using System.Data;
using System.Text.RegularExpressions;
using Back.Models;
using Back.Models.PostDto;
using Back.Services;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace back.Services;

public class PostService : IPostService
{
    private readonly DatabaseService _databaseService = DatabaseService.GetInstance();
    private readonly ITagService _tagService;
    private readonly UserService _userService;

    public PostService(ITagService tagService, UserService userService)
    {
        _tagService = tagService;
        _userService = userService;
    }

    //get post by id
    public Post GetPost(int id)
    {
        string query = "SELECT * FROM api_schema.post WHERE id = @id";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@id", id }
            };

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (!reader.HasRows) return null;
            reader.Read();

            return CompilePost(reader);
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    //get all user's posts
    public List<Post>? GetAllUserPosts(int userId)
    {
        string query = "SELECT * FROM api_schema.post WHERE user_id = @user_id";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@user_id", userId }
            };

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (!reader.HasRows) return null;

            var posts = new List<Post>();
            while (reader.Read())
            {
                var post = CompilePost(reader);
                posts.Add(post);
            }

            return posts;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public List<Post>? GetAllUserPosts(string username)
    {
        string query = @"
    SELECT p.id, p.user_id, p.post_name, p.post_text, p.container_id, p.post_date, p.likes, p.access_level
    FROM api_schema.post p
    JOIN api_schema.user u ON p.user_id = u.id
    WHERE u.username = @username";

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@username", username }
            };

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (!reader.HasRows) return null;

            var posts = new List<Post>();
            while (reader.Read())
            {
                var post = CompilePost(reader);
                posts.Add(post);
                Console.WriteLine(post.ToString());
            }

            return posts;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    //get newest posts by page
    public List<Post> GetNewestPosts(int pageNumber = 1, int pageSize = 10, string? tags = null,
        string? accessType = null)
    {
        string queryWithTags = @"
    SELECT p.id, p.user_id, p.post_name, p.post_text, p.container_id, p.post_date, p.likes, p.access_level
    FROM api_schema.post p
    LEFT JOIN api_schema.post_tags pt ON p.id = pt.post_id
    LEFT JOIN api_schema.tags t ON pt.tag_id = t.id
    WHERE t.tag_name = ANY(@tags)
    AND p.access_level = @accessType::api_schema.access_level
    ORDER BY p.post_date DESC
    LIMIT @pageSize OFFSET @offset";

        string queryWithoutTags = @"
    SELECT p.id, p.user_id, p.post_name, p.post_text, p.container_id, p.post_date, p.likes, p.access_level
    FROM api_schema.post p
    WHERE p.access_level = @accessType::api_schema.access_level
    ORDER BY p.post_date DESC
    LIMIT @pageSize OFFSET @offset";

        var offset = (pageNumber - 1) * pageSize;
        if (offset < 0) offset = 0;
        if (accessType != "public" && accessType != "protected" && accessType != "private") accessType = "public";

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@accessType", accessType },
                { "@pageSize", pageSize },
                { "@offset", offset }
            };

            if (!string.IsNullOrEmpty(tags))
            {
                parameters.Add("@tags", tags.Split(',').Select(tag => tag.Trim()).ToArray());
            }

            string query = string.IsNullOrEmpty(tags) ? queryWithoutTags : queryWithTags;

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            var posts = new List<Post>();
            while (reader.Read())
            {
                var post = CompilePost(reader);
                posts.Add(post);
                Console.WriteLine(post.ToString()); // Log each post to ensure it is being processed
            }

            return posts;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    //get al user's posts by page
    public List<PostMini>? GetUserPosts(string username, int pageNumber, int pageSize)
    {
        string query = @"
    SELECT p.id, p.post_name, i.image_file_path, p.likes
    FROM api_schema.post p
    JOIN api_schema.image_container c ON p.container_id = c.id
    JOIN api_schema.image i ON c.main_image_id = i.id
    JOIN api_schema.user u ON p.user_id = u.id
    WHERE u.username = @username
    ORDER BY p.post_date DESC
    LIMIT @pageSize OFFSET @offset";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var offset = ((pageNumber - 1) * pageSize);
            offset = offset < 0 ? 0 : offset;
            var parameters = new Dictionary<string, object>
            {
                { "@username", username },
                { "@pageSize", pageSize },
                { "@offset", offset }
            };
            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);

            if (!reader.HasRows) return null;

            var posts = new List<PostMini>();
            while (reader.Read())
            {
                posts.Add(new PostMini
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    MainImageFilePath = reader.GetString(2),
                    Likes = reader.GetInt32(3)
                });
            }

            return posts;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }


    //delete post by id
    public bool DeletePost(int id, string username)
    {
        string authorIdQuery = @"
    SELECT user_id FROM api_schema.post WHERE id = @id";

        string userIdQuery = @"SELECT id FROM api_schema.user WHERE username = @username";

        string deletePostTagsQuery = "DELETE FROM api_schema.post_tags WHERE post_id = @post_id";

        string deletePowerupQuery = "DELETE FROM api_schema.powerup WHERE post_id = @post_id";

        string deletePostReportQuery = "DELETE FROM api_schema.post_report WHERE reported_id = @post_id";

        string deletePostPopularityQuery = "DELETE FROM api_schema.post_popularity WHERE post_id = @post_id";

        string deleteQuery = "DELETE FROM api_schema.post WHERE id = @id";

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;

        int authorId = 0;
        int userId = 0;

        try
        {
            // Retrieve author ID
            var parameters = new Dictionary<string, object>
            {
                { "@id", id }
            };
            using (var reader = _databaseService.ExecuteQuery(authorIdQuery, out connection, out command, parameters))
            {
                if (!reader.HasRows) return false;
                reader.Read();
                authorId = reader.GetInt32(0);
            }

            // Retrieve user ID
            parameters = new Dictionary<string, object>
            {
                { "@username", username }
            };
            using (var reader = _databaseService.ExecuteQuery(userIdQuery, out connection, out command, parameters))
            {
                if (!reader.HasRows) return false;
                reader.Read();
                userId = reader.GetInt32(0);

                // Check if the user is the author
                if (authorId != userId) return false;
            }

            // Delete related post tags
            parameters = new Dictionary<string, object>
            {
                { "@post_id", id }
            };
            using (command = new NpgsqlCommand(deletePostTagsQuery, connection))
            {
                command.Parameters.AddWithValue("@post_id", id);
                command.ExecuteNonQuery();
            }

            // Delete related powerups
            using (command = new NpgsqlCommand(deletePowerupQuery, connection))
            {
                command.Parameters.AddWithValue("@post_id", id);
                command.ExecuteNonQuery();
            }

            // Delete related post reports
            using (command = new NpgsqlCommand(deletePostReportQuery, connection))
            {
                command.Parameters.AddWithValue("@post_id", id);
                command.ExecuteNonQuery();
            }

            // Delete related post popularity
            using (command = new NpgsqlCommand(deletePostPopularityQuery, connection))
            {
                command.Parameters.AddWithValue("@post_id", id);
                command.ExecuteNonQuery();
            }

            // Delete the post
            using (command = new NpgsqlCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    //compile post from all data needed
    public List<Tag> GetPostTags(int id)
    {
        string query = @"
        SELECT t.id, t.tag_name, t.tag_type
        FROM api_schema.post_tags pt
        JOIN api_schema.tags t ON pt.tag_id = t.id
        WHERE pt.post_id = @post_id";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@post_id", id }
            };

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            var tags = new List<Tag>();
            while (reader.Read())
            {
                tags.Add(new Tag(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
            }

            return tags;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public ImageContainer GetPostImages(int id)
    {
        string query = @"
        SELECT i.id, i.image_file_path
        FROM api_schema.post p
        JOIN api_schema.image i ON p.container_id = i.container_id
        WHERE p.id = @id";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@id", id }
            };

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (!reader.HasRows) return null;

            var images = new List<Image>();
            while (reader.Read())
            {
                images.Add(new Image(reader.GetInt32(0), reader.GetString(1)));
            }

            string mainImageQuery = @"
            SELECT i.id, i.image_file_path
            FROM api_schema.post p
            JOIN api_schema.image_container c ON p.container_id = c.id
            JOIN api_schema.image i ON c.main_image_id = i.id
            WHERE p.id = @id";

            using var mainImageReader =
                _databaseService.ExecuteQuery(mainImageQuery, out connection, out command, parameters);
            if (!mainImageReader.HasRows) return new ImageContainer(id, images.FirstOrDefault(), images);
            mainImageReader.Read();
            var container = new ImageContainer(id, new Image(mainImageReader.GetInt32(0), mainImageReader.GetString(1)),
                images);

            return container;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public List<Rating> GetPostRatings(int id)
    {
        string query = @"
        SELECT p.rating, t.id, t.tag_name, t.tag_type
        FROM api_schema.post_popularity p
        JOIN api_schema.popular_list l ON p.list_id = l.id
        JOIN api_schema.tags t ON l.tag_id = t.id
        WHERE p.post_id = @post_id";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@post_id", id }
            };

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            var ratings = new List<Rating>();
            while (reader.Read())
            {
                ratings.Add(new Rating(reader.GetInt32(0),
                    new Tag(reader.GetInt32(1), reader.GetString(2), reader.GetString(3))));
            }

            return ratings;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    private Post CompilePost(NpgsqlDataReader reader)
    {
        return new Post(
            reader.GetInt32(0),
            _userService.GetUser(reader.GetInt32(1)),
            reader.GetString(2),
            reader.GetString(3),
            GetPostImages(reader.GetInt32(0)),
            DateOnly.FromDateTime(reader.GetDateTime(5)),
            reader.GetInt32(6),
            reader.GetString(7),
            _tagService.GetPostTags(reader.GetInt32(0)),
            GetPostRatings(reader.GetInt32(0))
        );
    }
}