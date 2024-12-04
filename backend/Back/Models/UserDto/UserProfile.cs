namespace Back.Models
{
    public class UserProfile
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Dictionary<string, int> RatingPositions { get; set; } // Key: Tag, Value: Position
        public string Description { get; set; }
        public string ProfileImage { get; set; }
        public int? Rubies { get; set; }

        public UserProfile(string username, string firstName, string lastName, Dictionary<string, int> ratingPositions,
            string description, string profileImage, int? rubies = null)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            RatingPositions = ratingPositions;
            Description = description;
            ProfileImage = profileImage;
            Rubies = rubies;
        }
    }
}