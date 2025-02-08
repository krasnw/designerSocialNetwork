using Back.Models.UserDto;
using Back.Models.PostDto;
namespace Back.Models
{
    public class CreateUserReportRequest
    {
        public string Username { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
    }

    public class CreatePostReportRequest
    {
        public int PostId { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
    }

    public class ReasonResponse
    {
        public int Id { get; set; }
        public string ReasonName { get; set; }
    }
}
