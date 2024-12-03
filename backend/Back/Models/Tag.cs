namespace Back.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TagType { get; set; }
    
    public Tag(int id, string name, string tagType)
    {
        Id = id;
        Name = name;
        TagType = tagType;
    }
}