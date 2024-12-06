using Back.Models;
using Back.Services.Interfaces;
using Npgsql;

namespace Back.Services;

public class TagService : ITagService
{
    private readonly DatabaseService _databaseService;

    public TagService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    private List<Tag> ExecuteTagQuery(string query, Dictionary<string, object> parameters = null)
    {
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
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

    public List<Tag> GetAllTags()
    {
        return ExecuteTagQuery("SELECT * FROM api_schema.tags");
    }

    public List<Tag> GetAllTags(string type)
    {
        return ExecuteTagQuery(
            "SELECT * FROM api_schema.tags WHERE tag_type = @type",
            new Dictionary<string, object> { { "@type", type } }
        );
    }

    public List<Tag> GetAllUserTags(string username)
    {
        return ExecuteTagQuery(@"
            SELECT DISTINCT t.id, t.tag_name, t.tag_type
            FROM api_schema.tags t
            JOIN api_schema.post_tags pt ON t.id = pt.tag_id
            JOIN api_schema.post p ON pt.post_id = p.id
            JOIN api_schema.user u ON p.user_id = u.id
            WHERE u.username = @username",
            new Dictionary<string, object> { { "@username", username } }
        );
    }

    public List<Tag> GetPostTags(int postId)
    {
        return ExecuteTagQuery(@"
            SELECT t.id, t.tag_name, t.tag_type
            FROM api_schema.post_tags pt
            JOIN api_schema.tags t ON pt.tag_id = t.id
            WHERE pt.post_id = @post_id",
            new Dictionary<string, object> { { "@post_id", postId } }
        );
    }
}
