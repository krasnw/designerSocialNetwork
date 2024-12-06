using Back.Models;
using Back.Services.Interfaces;
using Npgsql;

namespace Back.Services;

public class TagService : ITagService
{
    private readonly DatabaseService _databaseService = DatabaseService.GetInstance();

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
            var parameters = new Dictionary<string, object> { { "@type", type } };
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

    public List<Tag> GetAllUserTags(string username)
    {
        string query = @"
            SELECT DISTINCT t.id, t.tag_name, t.tag_type
            FROM api_schema.tags t
            JOIN api_schema.post_tags pt ON t.id = pt.tag_id
            JOIN api_schema.post p ON pt.post_id = p.id
            JOIN api_schema.user u ON p.user_id = u.id
            WHERE u.username = @username";

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object> { { "@username", username } };
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

    public List<Tag> GetPostTags(int postId)
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
            var parameters = new Dictionary<string, object> { { "@post_id", postId } };
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
}
