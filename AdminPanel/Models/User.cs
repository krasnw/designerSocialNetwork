namespace AdminPanel.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public string AccountLevel { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public decimal AccessFee { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;

    public User() { }

    public User(
        string username,
        string email,
        string firstName,
        string lastName,
        string phoneNumber,
        decimal accessFee,
        string accountStatus,
        string accountLevel,
        string description,
        DateTime joinDate,
        string profileImage)
    {
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        AccessFee = accessFee;
        AccountStatus = accountStatus;
        AccountLevel = accountLevel;
        Description = description;
        JoinDate = joinDate;
        ProfileImage = profileImage;
    }
}