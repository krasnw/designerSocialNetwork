namespace Back.Models.PostDto;

public class PostMini
{
    /*main pic
     title
     likes
     post id*/
    public long Id { get; set; }
    public string Title { get; set; }
    public string MainImageFilePath { get; set; }
    public long Likes { get; set; }
    
    public static PostMini MapToPostMini(Post post)
    {
        return new PostMini
        {
            Id = post.Id,
            Title = post.Title,
            MainImageFilePath = post.Images.MainImage.ImageFilePath,
            Likes = post.Likes
        };
    }
}