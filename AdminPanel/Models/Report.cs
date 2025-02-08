namespace AdminPanel.Models;

public abstract class Report
{
    public long Id { get; set; }
    public string ReportReason { get; set; }
    public DateTime ReportDate { get; set; }
}

public class UserReport : Report
{
    public UserMiniDTO ReportedUser { get; set; }
    public UserMiniDTO Reporter { get; set; }
}

public class PostReport : Report
{
    public PostMiniDTO ReportedPost { get; set; }
    public UserMiniDTO ReportedUser { get; set; }
    public UserMiniDTO Reporter { get; set; }
}

public class UserMiniDTO
{
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ProfileImage { get; set; }
    public string? profileDescription { get; set; }

    public UserMiniDTO(string username, string firstName, string lastName, string profileImage, string? profileDescription)
    {
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        ProfileImage = profileImage;
        this.profileDescription = profileDescription;
    }
}

public class PostMiniDTO
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string? Content { get; set; }
    public ImageContainer Images { get; set; }

    public PostMiniDTO(long id, string title, string? content, ImageContainer images)
    {
        Id = id;
        Title = title;
        Content = content;
        Images = images;
    }
}

public class ImageContainer
{
    public string MainImage { get; set; }
    public List<string> Images { get; set; }

    public ImageContainer(string mainImage, List<string> images)
    {
        MainImage = mainImage;
        Images = images;
    }
}
