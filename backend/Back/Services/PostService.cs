using System.Data;
using System.Text.RegularExpressions;
using Back.Models;
using Back.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace back.Services;

public class PostService
{
    private DatabaseService _databaseService = DatabaseService.GetInstance();

    //get all tags
    public List<Tag> GetAllTags()
    {
        string query = "SELECT * FROM api_schema.tags";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            using var reader = _databaseService.ExecuteQuery(query, out connection, out command);
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

    public List<Tag> GetAllTags(string type)
    {
        string query = "SELECT * FROM api_schema.tags WHERE tag_type = @type";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@type", type }
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

    //get newest posts by page
    public List<Post>? GetNewestPosts(int pageNumber, int pageSize)
    {
        string query = @"
        SELECT * FROM api_schema.post
        ORDER BY post_date DESC
        LIMIT @pageSize OFFSET @offset";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@pageSize", pageSize },
                { "@offset", (pageNumber - 1) * pageSize }
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
        var post = new Post(
            reader.GetInt32(0), // id
            UserService.GetUser(reader.GetInt32(1)), // user
            reader.GetString(2), // title
            reader.GetString(3), // content
            GetPostImages(reader.GetInt32(0)), // images
            DateOnly.FromDateTime(reader.GetDateTime(5)), // post_date
            reader.GetInt32(6), // likes
            reader.GetString(7), // access_level
            GetPostTags(reader.GetInt32(0)), // tags
            GetPostRatings(reader.GetInt32(0)) // ratings
        );
        return post;
    }
}