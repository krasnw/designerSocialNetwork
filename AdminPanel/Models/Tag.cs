namespace AdminPanel.Models;

public enum TagType
{
    UiElement,
    Style,
    Color
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TagType Type { get; set; }
}
