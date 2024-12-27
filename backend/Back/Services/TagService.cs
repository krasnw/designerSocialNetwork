using Back.Models;
using Back.Services.Interfaces;
using Npgsql;

namespace Back.Services;

public class TagService : ITagService
{
    private readonly IDatabaseService _databaseService;

    public TagService(IDatabaseService databaseService)
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
                var tagTypeStr = reader.GetString(2);
                Console.WriteLine($"Database tag type: {tagTypeStr}"); // Debug log
                
                var enumTagType = tagTypeStr.ToUpper().Replace(" ", "_");
                Console.WriteLine($"Converted to enum: {enumTagType}"); // Debug log
                
                if (Enum.TryParse<TagType>(enumTagType, out var tagType))
                {
                    tags.Add(new Tag(reader.GetInt32(0), reader.GetString(1), tagType));
                }
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
        Console.WriteLine($"Input type: {type}"); // Debug log
        
        var query = @"
            SELECT t.* 
            FROM api_schema.tags t 
            WHERE LOWER(t.tag_type::text) = LOWER(@type)";
            
        var parameters = new Dictionary<string, object> { { "@type", type } };
        
        var tags = ExecuteTagQuery(query, parameters);
        Console.WriteLine($"Found {tags.Count} tags with type {type}"); // Debug log
        return tags;
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
