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
    string description,
    string profileImage)
{
    public string Username { get; set; } = username;
    public string Email { get; set; } = email;
    public string Password { get; set; } = password;
    public string FirstName { get; set; } = firstName;
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
    public string Description { get; set; } = description;
    public string ProfileImage { get; set; } = profileImage;

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
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }
    
    public class EditDataResponse
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
        public string? ProfileImage { get; set; }  // This remains string for existing image path
        public int? AccessFee { get; set; }
    }

    public class EditRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
        public IFormFile? ProfileImage { get; set; }  // This remains IFormFile for file upload
        public int? AccessFee { get; set; }
    }

    public class EditBasicRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Description { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }

    public class EditSensitiveRequest
    {
        public string CurrentPassword { get; set; } // Required for verification
        public string? NewUsername { get; set; }
        public string? NewEmail { get; set; }
        public string? NewPassword { get; set; }
        public string? NewPhoneNumber { get; set; }
        public int? NewAccessFee { get; set; }
    }
}