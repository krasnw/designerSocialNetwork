using System.Text.RegularExpressions;

namespace Back.Services;

public class UserService
{
    private readonly Dictionary<string, string> _users = new();
    private readonly HashSet<string> _loggedInUsers = new();

    public bool SignUp(string username, string email, string password, string firstName, string lastName,
        string phoneNumber)
    {
        ValidateSignUpData(username, email, password, firstName, lastName, phoneNumber);

        string joinDate = DateTime.Now.ToString("dd.MM.yyyy");
        int accessFee = -1; // -1 means not set
        string accountStatus = "active";
        string accountLevel = "user";

        string query = $"INSERT INTO api_schema.\"user\" " +
                       "(username, email, user_password, first_name, last_name, phone_number, join_date," +
                       " account_status, account_level, access_fee)" +
                       " VALUES ('{username}', '{email}', '{password}', '{firstName}', '{lastName}', '{phoneNumber}'," +
                       " '{joinDate}', '{accountStatus}', '{accountLevel}', {accessFee});";

        return _users.TryAdd(username, password);
    
    }

    private void ValidateSignUpData(string username, string email, string password, string firstName, string lastName,
        string phoneNumber)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phoneNumber))
        {
            throw new ArgumentException("All fields are required.");
        }

        var usernameRegex = new Regex(@"^[a-zA-Z0-9_]{3,50}$");
        if (!usernameRegex.IsMatch(username))
        {
            throw new ArgumentException("Invalid username format.");
        }

        var emailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        if (!emailRegex.IsMatch(email))
        {
            throw new ArgumentException("Invalid email format.");
        }

        var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$");
        if (!passwordRegex.IsMatch(password))
        {
            throw new ArgumentException(
                "Password must be at least 8 characters long and include a mix of upper and lower case letters and numbers.");
        }

        var phoneNumberRegex = new Regex(@"^\+?[0-9]{6,25}$");
        if (!phoneNumberRegex.IsMatch(phoneNumber))
        {
            throw new ArgumentException("Invalid phone number format.");
        }

        if (_users.ContainsKey(username))
        {
            throw new ArgumentException("Username already exists.");
        }
    }

    public bool IsSignedUp(string username)
    {
        return _users.ContainsKey(username);
    }

    public bool Login(string username, string password)
    {
        if (_users.TryGetValue(username, out var storedPassword) && storedPassword == password)
        {
            _loggedInUsers.Add(username);
            return true;
        }

        return false;
    }

    public bool Logout(string username)
    {
        return _loggedInUsers.Remove(username);
    }

    public bool IsLoggedIn(string username)
    {
        return _loggedInUsers.Contains(username);
    }
}