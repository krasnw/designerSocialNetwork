using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Back.Models;
using Back.Models.PostDto;
using Back.Services;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Back.Services;

public class PostService : IPostService
{
    private readonly IDatabaseService _databaseService;
    private readonly ITagService _tagService;
    private readonly IUserService _userService;

    private const string POST_BASE_SELECT = @"
        SELECT p.id, p.user_id, p.post_name, p.post_text, p.container_id, p.post_date, p.likes, p.access_level
        FROM api_schema.post p";

    public PostService(IDatabaseService databaseService, ITagService tagService, IUserService userService)
    {
        _databaseService = databaseService;
        _tagService = tagService;
        _userService = userService;
    }

    private T ExecuteQueryWithDisposable<T>(string query, Dictionary<string, object> parameters, Func<DbDataReader, T> processor)
    {
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            return processor(reader);
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    private Dictionary<string, object> CreatePostIdParam(long id) => 
        new() { { "@id", id } };

    public Post GetPost(long id) =>
        ExecuteQueryWithDisposable(
            $"{POST_BASE_SELECT} WHERE id = @id",
            CreatePostIdParam(id),
            reader => !reader.HasRows ? null : (reader.Read() ? CompilePost(reader) : null)
        );

    public List<Post>? GetAllUserPosts(int userId) =>
        ExecuteQueryWithDisposable(
            $"{POST_BASE_SELECT} WHERE user_id = @user_id",
            new Dictionary<string, object> { { "@user_id", userId } },
            reader => !reader.HasRows ? null : ReadMultiplePosts(reader)
        );

    public List<Post>? GetAllUserPosts(string username) =>
        ExecuteQueryWithDisposable(
            $"{POST_BASE_SELECT} JOIN api_schema.user u ON p.user_id = u.id WHERE u.username = @username",
            new Dictionary<string, object> { { "@username", username } },
            reader => !reader.HasRows ? null : ReadMultiplePosts(reader)
        );

    private List<Post> ReadMultiplePosts(DbDataReader reader)
    {
        var posts = new List<Post>();
        while (reader.Read())
        {
            var post = CompilePost(reader);
            posts.Add(post);
        }
        return posts;
    }

    //get newest posts by page
    public List<Post> GetNewestPosts(int pageNumber = 1, int pageSize = 10, string? tags = null,
        string? accessType = null, string? currentUser = null)
    {
        string queryWithTags = @"
    SELECT DISTINCT p.id, p.user_id, p.post_name, p.post_text, p.container_id, p.post_date, p.likes, p.access_level
    FROM api_schema.post p
    LEFT JOIN api_schema.post_tags pt ON p.id = pt.post_id
    LEFT JOIN api_schema.tags t ON pt.tag_id = t.id
    WHERE t.tag_name = ANY(@tags)
    AND CASE 
        WHEN @accessType = 'public' THEN p.access_level = 'public'
        WHEN @accessType = 'private' THEN (
            p.access_level = 'private'
            AND EXISTS (
                SELECT 1 FROM api_schema.private_access pa
                JOIN api_schema.user u ON pa.buyer_id = u.id
                WHERE u.username = @currentUser
                AND pa.seller_id = p.user_id
                AND pa.access_date > CURRENT_DATE
            )
        )
        ELSE (
            p.access_level = 'public'
            OR (
                p.access_level = 'private'
                AND EXISTS (
                    SELECT 1 FROM api_schema.private_access pa
                    JOIN api_schema.user u ON pa.buyer_id = u.id
                    WHERE u.username = @currentUser
                    AND pa.seller_id = p.user_id
                    AND pa.access_date > CURRENT_DATE
                )
            )
        )
    END
    AND NOT EXISTS (
        SELECT 1 FROM api_schema.user u 
        WHERE u.id = p.user_id 
        AND u.username = @currentUser
    )
    ORDER BY p.post_date DESC
    LIMIT @pageSize OFFSET @offset";

        string queryWithoutTags = @"
    SELECT DISTINCT p.id, p.user_id, p.post_name, p.post_text, p.container_id, p.post_date, p.likes, p.access_level
    FROM api_schema.post p
    WHERE CASE 
        WHEN @accessType = 'public' THEN p.access_level = 'public'
        WHEN @accessType = 'private' THEN (
            p.access_level = 'private'
            AND EXISTS (
                SELECT 1 FROM api_schema.private_access pa
                JOIN api_schema.user u ON pa.buyer_id = u.id
                WHERE u.username = @currentUser
                AND pa.seller_id = p.user_id
                AND pa.access_date > CURRENT_DATE
            )
        )
        ELSE (
            p.access_level = 'public'
            OR (
                p.access_level = 'private'
                AND EXISTS (
                    SELECT 1 FROM api_schema.private_access pa
                    JOIN api_schema.user u ON pa.buyer_id = u.id
                    WHERE u.username = @currentUser
                    AND pa.seller_id = p.user_id
                    AND pa.access_date > CURRENT_DATE
                )
            )
        )
    END
    AND NOT EXISTS (
        SELECT 1 FROM api_schema.user u 
        WHERE u.id = p.user_id 
        AND u.username = @currentUser
    )
    ORDER BY p.post_date DESC
    LIMIT @pageSize OFFSET @offset";

        var offset = (pageNumber - 1) * pageSize;
        if (offset < 0) offset = 0;

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            string query;
            var parameters = new Dictionary<string, object>
            {
                { "@pageSize", pageSize },
                { "@offset", offset },
                { "@accessType", accessType ?? "all" }
            };

            if (!string.IsNullOrEmpty(currentUser))
            {
                parameters.Add("@currentUser", currentUser);
            }

            // Select query based on tags
            query = string.IsNullOrEmpty(tags) ? queryWithoutTags : queryWithTags;
            if (!string.IsNullOrEmpty(tags))
            {
                parameters.Add("@tags", tags.Split(',').Select(tag => tag.Trim()).ToArray());
            }

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
    public List<PostMini>? GetUserPosts(string username, string? currentUser, int pageNumber, int pageSize, string? tags = null, string? accessType = null)
{
    string query = @"
    WITH post_tags AS (
        SELECT pt.post_id, array_agg(t.tag_name) as tags
        FROM api_schema.post_tags pt
        JOIN api_schema.tags t ON pt.tag_id = t.id
        GROUP BY pt.post_id
    )
    SELECT p.id, p.post_name, p.access_level, 
           COALESCE(i.image_file_path, 'default.jpg') as image_file_path, 
           p.likes,
           COALESCE(pt.tags, ARRAY[]::text[]) as tags
    FROM api_schema.post p
    JOIN api_schema.user u ON p.user_id = u.id
    LEFT JOIN api_schema.image_container c ON p.container_id = c.id
    LEFT JOIN api_schema.image i ON c.main_image_id = i.id
    LEFT JOIN post_tags pt ON p.id = pt.post_id
    WHERE u.username = @username
    {0}
    {1}
    ORDER BY p.post_date DESC
    LIMIT @pageSize OFFSET @offset";

    var offset = (pageNumber - 1) * pageSize;
    if (offset < 0) offset = 0;

    var parameters = new Dictionary<string, object>
    {
        { "@username", username },
        { "@pageSize", pageSize },
        { "@offset", offset }
    };

    // Build access filter
    string accessFilter = "";
    if (string.IsNullOrEmpty(currentUser))
    {
        accessFilter = "AND p.access_level = 'public'";
    }
    else if (currentUser != username)
    {
        accessFilter = @"AND (p.access_level = 'public' 
            OR (p.access_level = 'private' 
                AND EXISTS (
                    SELECT 1 FROM api_schema.private_access pa 
                    JOIN api_schema.user u2 ON pa.buyer_id = u2.id 
                    WHERE u2.username = @currentUser 
                    AND pa.seller_id = p.user_id
                )
            ))";
        parameters.Add("@currentUser", currentUser);
    }

    // Add access type filter if specified
    if (!string.IsNullOrEmpty(accessType))
    {
        accessFilter += " AND p.access_level = @accessType";
        parameters.Add("@accessType", accessType.ToLower());
    }

    // Build tag filter
    string tagFilter = "";
    if (!string.IsNullOrEmpty(tags))
    {
        var tagList = tags.Split(',').Select(t => t.Trim().ToLower()).ToArray();
        tagFilter = "HAVING array_agg(DISTINCT LOWER(t.tag_name)) && @tagList";
        parameters.Add("@tagList", tagList);
    }

    // Format the final query
    query = string.Format(query, accessFilter, tagFilter);

    NpgsqlConnection connection = null;
    NpgsqlCommand command = null;

    try
    {
        using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
        
        if (!reader.HasRows) return null;

        var posts = new List<PostMini>();
        while (reader.Read())
        {
            posts.Add(new PostMini
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Access = reader.GetString(2),
                MainImageFilePath = reader.GetString(3),
                Likes = reader.GetInt32(4),
                Tags = reader.GetValue(5) != DBNull.Value 
                    ? ((string[])reader.GetValue(5)).Where(t => t != null).ToList() 
                    : new List<string>()
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

    //check if user has access to post
    public bool HasUserAccessToPost(string username, long postId)
    {
        string query = @"
            SELECT COUNT(*)
            FROM api_schema.private_access pa
            JOIN api_schema.user u ON pa.buyer_id = u.id
            WHERE u.username = @username 
            AND pa.seller_id = (SELECT user_id FROM api_schema.post WHERE id = @postId)";
    
        var parameters = new Dictionary<string, object>
        {
            { "@username", username },
            { "@postId", postId }
        };
    
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (reader.Read())
            {
                return reader.GetInt32(0) > 0;
            }
            return false;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    //delete post by id
    public bool DeletePost(long id, string username)
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
    public List<Tag> GetPostTags(long id)
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
                var tagTypeStr = reader.GetString(2).ToUpper().Replace(" ", "_");
                Enum.TryParse<TagType>(tagTypeStr, out var tagType);
                tags.Add(new Tag(reader.GetInt32(0), reader.GetString(1), tagType));
            }

            return tags;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public ImageContainer GetPostImages(long id)
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
                images.Add(new Image((int)reader.GetInt64(0), reader.GetString(1)));  // Add cast to int
            }

            string mainImageQuery = @"
            SELECT i.id, i.image_file_path
            FROM api_schema.post p
            JOIN api_schema.image_container c ON p.container_id = c.id
            JOIN api_schema.image i ON c.main_image_id = i.id
            WHERE p.id = @id";

            using var mainImageReader =
                _databaseService.ExecuteQuery(mainImageQuery, out connection, out command, parameters);
            if (!mainImageReader.HasRows) return new ImageContainer((int)id, images.FirstOrDefault(), images);  // Add cast to int
            mainImageReader.Read();
            var container = new ImageContainer(
                (int)id, 
                new Image((int)mainImageReader.GetInt64(0), mainImageReader.GetString(1)),
                images
            );

            return container;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public List<Rating> GetPostRatings(long id)
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
                var tagTypeStr = reader.GetString(3).ToUpper().Replace(" ", "_");
                Enum.TryParse<TagType>(tagTypeStr, out var tagType);
                ratings.Add(new Rating(reader.GetInt32(0),
                    new Tag(reader.GetInt32(1), reader.GetString(2), tagType)));
            }

            return ratings;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    private Post CompilePost(DbDataReader reader) =>
        new(
            reader.GetInt32(0),
            _userService.GetUser(reader.GetInt32(1)),
            reader.GetString(2),
            reader.GetString(3),
            GetPostImages(reader.GetInt32(0)),
            reader.GetDateTime(5),  // Changed from DateOnly.FromDateTime()
            reader.GetInt32(6),
            reader.GetString(7),
            _tagService.GetPostTags(reader.GetInt32(0)),
            GetPostRatings(reader.GetInt32(0))
        );

    private Post CompilePost(DbDataReader reader, string? currentUser = null)
    {
        var postId = reader.GetInt32(0);
        return new Post(
            postId,
            _userService.GetUser(reader.GetInt32(1)),
            reader.GetString(2),
            reader.GetString(3),
            GetPostImages(postId),
            reader.GetDateTime(5),
            reader.GetInt32(6),
            reader.GetString(7),
            _tagService.GetPostTags(postId),
            GetPostRatings(postId)
        );
    }

    public async Task<Post?> CreatePost(string username, PostCreationData request)
    {
        if (request.ImagePaths.Count > 10)
            throw new ArgumentException("Maximum 10 images allowed per post");

        try
        {
            // 1. Create image container
            var containerId = CreateImageContainer(request.ImagePaths, request.MainImagePath, username);
            if (containerId <= 0) throw new Exception("Failed to create image container");

            // 2. Create post record
            var postId = CreatePostRecord(username, request, containerId);
            if (postId <= 0) throw new Exception("Failed to create post record");

            // 3. Add tags
            AddPostTags(postId, request.Tags);

            // 4. Return the created post
            var post = GetPost(postId);
            if (post == null) throw new Exception("Post creation failed, post is null");

            // Generate protected access hash if post is protected
            if (post.Access.Equals("protected", StringComparison.OrdinalIgnoreCase))
            {
                var hash = GenerateProtectedAccessHash(post.Id);
                // Add hash to response or handle it as needed
            }

            return post;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreatePost: {ex.Message}");
            throw new Exception($"Failed to create post: {ex.Message}", ex);
        }
    }

    private long CreateImageContainer(List<string> imagePaths, string mainImagePath, string username)
    {
        if (imagePaths == null || !imagePaths.Any())
            throw new ArgumentException("Image paths cannot be null or empty");

        if (string.IsNullOrEmpty(mainImagePath))
            throw new ArgumentException("Main image path cannot be null or empty");

        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username cannot be null or empty");

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        var containerId = 0;

        try
        {
            // 1. Get user ID first
            var userQuery = "SELECT id FROM api_schema.user WHERE username = @username";
            var userParams = new Dictionary<string, object>
            {
                { "@username", username }
            };

            int userId;
            using (var userReader = _databaseService.ExecuteQuery(userQuery, out connection, out command, userParams))
            {
                if (!userReader.Read())
                    throw new Exception("User not found");
                userId = userReader.GetInt32(0);
            }

            // 2. Create container
            var containerQuery = @"
                INSERT INTO api_schema.image_container (amount_of_images)
                VALUES (@amount)
                RETURNING id";
            
            var containerParams = new Dictionary<string, object>
            {
                { "@amount", imagePaths.Count }
            };

            using (var containerReader = _databaseService.ExecuteQuery(containerQuery, out connection, out command, containerParams))
            {
                if (!containerReader.Read())
                    throw new Exception("Failed to create image container");
                containerId = containerReader.GetInt32(0);
            }

            // 3. Insert or update images
            foreach (var path in imagePaths)
            {
                var imageQuery = @"
                    INSERT INTO api_schema.image (image_file_path, container_id, user_id)
                    VALUES (@path, @containerId, @userId)
                    ON CONFLICT (image_file_path) DO UPDATE 
                    SET container_id = @containerId,
                        user_id = @userId
                    RETURNING id";

                var imageParams = new Dictionary<string, object>
                {
                    { "@path", path },
                    { "@containerId", containerId },
                    { "@userId", userId }
                };

                using var imageReader = _databaseService.ExecuteQuery(imageQuery, out connection, out command, imageParams);
                if (!imageReader.Read())
                    throw new Exception($"Failed to process image: {path}");
            }

            // 4. Set main image
            var mainImageQuery = @"
                UPDATE api_schema.image_container 
                SET main_image_id = (
                    SELECT id FROM api_schema.image 
                    WHERE image_file_path = @path 
                    AND container_id = @containerId
                )
                WHERE id = @containerId
                RETURNING id";

            var mainImageParams = new Dictionary<string, object>
            {
                { "@path", mainImagePath },
                { "@containerId", containerId }
            };

            using var mainImageReader = _databaseService.ExecuteQuery(mainImageQuery, out connection, out command, mainImageParams);
            if (!mainImageReader.Read())
                throw new Exception("Failed to set main image");

            return containerId;
        }
        catch (Exception ex)
        {
            // If anything fails, try to cleanup the container
            if (containerId > 0)
            {
                try
                {
                    var cleanupQuery = "DELETE FROM api_schema.image_container WHERE id = @containerId";
                    var cleanupParams = new Dictionary<string, object> { { "@containerId", containerId } };
                    _databaseService.ExecuteNonQuery(cleanupQuery, cleanupParams);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            throw new Exception($"Failed to create image container: {ex.Message}");
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    private long CreatePostRecord(string username, PostCreationData request, long containerId)
    {
        // Add null checks and default values
        var accessLevel = string.IsNullOrEmpty(request.AccessLevel) ? "public" : request.AccessLevel.ToLower();
        var title = request.Title ?? throw new ArgumentException("Title cannot be null");

        var query = @"
            INSERT INTO api_schema.post (
                user_id, post_name, post_text, container_id, 
                post_date, likes, access_level
            )
            SELECT 
                u.id, @title, COALESCE(@content, ''), @containerId, 
                CURRENT_TIMESTAMP, 0, @accessLevel::api_schema.access_level  -- Changed from CURRENT_DATE
            FROM api_schema.user u
            WHERE u.username = @username
            RETURNING id";

        var parameters = new Dictionary<string, object>
        {
            { "@username", username },
            { "@title", title },
            { "@content", (object?)request.Content ?? DBNull.Value },
            { "@containerId", containerId },
            { "@accessLevel", accessLevel }
        };

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;

        try
        {
            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            throw new Exception("Failed to create post record");
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    private void AddPostTags(long postId, List<string> tags)
    {
        // Add null check for tags
        if (tags == null) return;

        foreach (var tag in tags)
        {
            if (string.IsNullOrEmpty(tag)) continue;

            // First ensure tag exists
            var tagQuery = @"
                INSERT INTO api_schema.tags (tag_name, tag_type)
                VALUES (@name, 'style')
                ON CONFLICT (tag_name) DO UPDATE 
                SET tag_name = EXCLUDED.tag_name
                RETURNING id";

            var tagParams = new Dictionary<string, object>
            {
                { "@name", tag.ToLower() }
            };

            int tagId;
            NpgsqlConnection connection = null;
            NpgsqlCommand command = null;

            try
            {
                using var reader = _databaseService.ExecuteQuery(tagQuery, out connection, out command, tagParams);
                if (reader.Read())
                {
                    tagId = reader.GetInt32(0);
                }
                else
                {
                    continue;
                }
            }
            finally
            {
                command?.Dispose();
                connection?.Dispose();
            }

            // Then add post-tag relationship
            var relationQuery = @"
                INSERT INTO api_schema.post_tags (post_id, tag_id)
                VALUES (@postId, @tagId)
                ON CONFLICT DO NOTHING";

            var relationParams = new Dictionary<string, object>
            {
                { "@postId", postId },
                { "@tagId", tagId }
            };

            _databaseService.ExecuteNonQuery(relationQuery, relationParams);
        }
    }

    public Post? GetProtectedPost(string hash)
    {
        string query = @"
            SELECT p.id, p.user_id, p.post_name, p.post_text, p.container_id, p.post_date, p.likes, p.access_level
            FROM api_schema.post p
            JOIN api_schema.protected_post_access ppa ON p.id = ppa.post_id
            WHERE ppa.access_hash = @hash";

        var parameters = new Dictionary<string, object>
        {
            { "@hash", hash }
        };

        return ExecuteQueryWithDisposable(query, parameters,
            reader => !reader.HasRows ? null : (reader.Read() ? CompilePost(reader) : null));
    }

    public string GenerateProtectedAccessHash(long postId)
    {
        var hash = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");

        string query = @"
            INSERT INTO api_schema.protected_post_access (post_id, access_hash)
            VALUES (@postId, @hash)";

        var parameters = new Dictionary<string, object>
        {
            { "@postId", postId },
            { "@hash", hash }
        };

        _databaseService.ExecuteNonQuery(query, parameters);
        return hash;
    }

    public bool LikePost(string username, long postId)
    {
        try
        {
            // First check if user is the post owner
            string ownerCheckQuery = @"
                SELECT EXISTS (
                    SELECT 1 
                    FROM api_schema.post p
                    JOIN api_schema.user u ON p.user_id = u.id
                    WHERE u.username = @username AND p.id = @postId
                )";

            var parameters = new Dictionary<string, object>
            {
                { "@username", username },
                { "@postId", postId }
            };

            NpgsqlConnection connection = null;
            NpgsqlCommand command = null;

            try
            {
                using (var reader = _databaseService.ExecuteQuery(ownerCheckQuery, out connection, out command, parameters))
                {
                    if (reader.Read() && reader.GetBoolean(0))
                    {
                        throw new InvalidOperationException("Cannot like your own post");
                    }
                }

                // Check if already liked
                string checkQuery = @"
                    SELECT EXISTS (
                        SELECT 1 
                        FROM api_schema.post_likes pl
                        JOIN api_schema.user u ON pl.user_id = u.id
                        WHERE u.username = @username AND pl.post_id = @postId
                    )";

                bool alreadyLiked = false;
                using (var reader = _databaseService.ExecuteQuery(checkQuery, out connection, out command, parameters))
                {
                    if (reader.Read())
                    {
                        alreadyLiked = reader.GetBoolean(0);
                    }
                }

                string query;
                if (alreadyLiked)
                {
                    // Unlike: Remove like and decrease counter
                    query = @"
                        DELETE FROM api_schema.post_likes pl
                        USING api_schema.user u
                        WHERE pl.user_id = u.id 
                        AND u.username = @username 
                        AND pl.post_id = @postId;

                        UPDATE api_schema.post
                        SET likes = likes - 1
                        WHERE id = @postId;";
                }
                else
                {
                    // Like: Add like and increase counter
                    query = @"
                        INSERT INTO api_schema.post_likes (user_id, post_id)
                        SELECT u.id, @postId
                        FROM api_schema.user u
                        WHERE u.username = @username;

                        UPDATE api_schema.post
                        SET likes = likes + 1
                        WHERE id = @postId;";
                }

                _databaseService.ExecuteNonQuery(query, parameters);
                return true;
            }
            finally
            {
                command?.Dispose();
                connection?.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LikePost: {ex.Message}");
            return false;
        }
    }

    public bool IsPostLikedByUser(string username, long postId)
    {
        string query = @"
            SELECT EXISTS (
                SELECT 1 
                FROM api_schema.post_likes pl
                JOIN api_schema.user u ON pl.user_id = u.id
                WHERE u.username = @username AND pl.post_id = @postId
            )";

        var parameters = new Dictionary<string, object>
        {
            { "@username", username },
            { "@postId", postId }
        };

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;

        try
        {
            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            return reader.Read() && reader.GetBoolean(0);
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public List<PostMini>? GetOwnPosts(string username, int pageNumber = 1, int pageSize = 10, string? tags = null, string? accessType = null)
{
    string query = @"
    WITH post_tags AS (
        SELECT pt.post_id, array_agg(t.tag_name) as tags
        FROM api_schema.post_tags pt
        JOIN api_schema.tags t ON pt.tag_id = t.id
        GROUP BY pt.post_id
    )
    SELECT p.id, p.post_name, p.access_level, 
           COALESCE(i.image_file_path, 'default.jpg') as image_file_path, 
           p.likes,
           COALESCE(pt.tags, ARRAY[]::text[]) as tags
    FROM api_schema.post p
    JOIN api_schema.user u ON p.user_id = u.id
    LEFT JOIN api_schema.image_container c ON p.container_id = c.id
    LEFT JOIN api_schema.image i ON c.main_image_id = i.id
    LEFT JOIN post_tags pt ON p.id = pt.post_id
    WHERE u.username = @username
    {0}
    {1}
    ORDER BY p.post_date DESC
    LIMIT @pageSize OFFSET @offset";

    var offset = (pageNumber - 1) * pageSize;
    if (offset < 0) offset = 0;

    var parameters = new Dictionary<string, object>
    {
        { "@username", username },
        { "@pageSize", pageSize },
        { "@offset", offset }
    };

    // Build access filter
    string accessFilter = "";
    if (!string.IsNullOrEmpty(accessType))
    {
        accessFilter = " AND p.access_level = @accessType";
        parameters.Add("@accessType", accessType.ToLower());
    }

    // Build tag filter
    string tagFilter = "";
    if (!string.IsNullOrEmpty(tags))
    {
        var tagList = tags.Split(',').Select(t => t.Trim().ToLower()).ToArray();
        tagFilter = " AND pt.tags && @tagList";
        parameters.Add("@tagList", tagList);
    }

    // Format the final query
    query = string.Format(query, accessFilter, tagFilter);

    NpgsqlConnection connection = null;
    NpgsqlCommand command = null;

    try
    {
        using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
        
        if (!reader.HasRows) return null;

        var posts = new List<PostMini>();
        while (reader.Read())
        {
            posts.Add(new PostMini
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Access = reader.GetString(2),
                MainImageFilePath = reader.GetString(3),
                Likes = reader.GetInt32(4),
                Tags = reader.GetValue(5) != DBNull.Value 
                    ? ((string[])reader.GetValue(5)).Where(t => t != null).ToList() 
                    : new List<string>()
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
}