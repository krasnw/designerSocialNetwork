namespace Back.Models;

public class User(
    string username,
    string email,
    string password,
    string firstName,
    string lastName,
    string phoneNumber,
    decimal accessFee,
    string accountStatus,
    string accountLevel,
    string? middleName = null)
{
    public string Username { get; set; } = username;
    public string Email { get; set; } = email;
    public string Password { get; set; } = password;
    public string FirstName { get; set; } = firstName;
    public string? MiddleName { get; set; } = middleName;
    public string LastName { get; set; } = lastName;
    public string PhoneNumber { get; set; } = phoneNumber;
    public DateTime JoinDate { get; set; } = DateTime.Now;
    public string AccountStatus { get; set; } = accountStatus;
    public string AccountLevel { get; set; } = accountLevel;
    public bool IsLoggedIn { get; set; } = false;
    public string Token { get; set; } = "";
    public DateTime LastLoginTime { get; set; } = DateTime.MinValue;
    public string LastLoginIP { get; set; } = "";
    public decimal AccessFee { get; set; } = accessFee;
    
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignUpRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }
}