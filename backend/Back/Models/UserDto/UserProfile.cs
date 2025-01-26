namespace Back.Models
{
    public class RatingEntry
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public RatingEntry(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }

    public class UserProfile
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public string ProfileImage { get; set; }
        public int? Rubies { get; set; }
        public int TotalLikes { get; set; }
        public int CompletedTasks { get; set; }

        public UserProfile(string username, string firstName, string lastName,
            string description, string profileImage, int? rubies = null, int totalLikes = 0, int completedTasks = 0)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            Description = description;
            ProfileImage = profileImage;
            Rubies = rubies;
            TotalLikes = totalLikes;
            CompletedTasks = completedTasks;
        }
    }
}