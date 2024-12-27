using System.Text.Json.Serialization;

namespace Back.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TagType
{
    UI_ELEMENT,
    STYLE,
    COLOR
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    private TagType _tagType;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TagType TagType
    {
        get => _tagType;
        set => _tagType = value;
    }
    
    public Tag(int id, string name, TagType tagType)
    {
        Id = id;
        Name = name;
        TagType = tagType;
    }
}