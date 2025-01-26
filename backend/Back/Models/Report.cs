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

    public class AllReportsResponse
    {
        public IEnumerable<UserReport> UserReports { get; set; }
        public IEnumerable<PostReport> PostReports { get; set; }

        public class UserReport
        {
            public int Id { get; set; }
            public string ReporterUsername { get; set; }
            public string ReportedUsername { get; set; }
            public string ReasonName { get; set; }
            public string Description { get; set; }
            public DateTime ReportDate { get; set; }
        }

        public class PostReport
        {
            public int Id { get; set; }
            public string ReporterUsername { get; set; }
            public int PostId { get; set; }
            public string ReasonName { get; set; }
            public string Description { get; set; }
            public DateTime ReportDate { get; set; }
        }
    }
}
