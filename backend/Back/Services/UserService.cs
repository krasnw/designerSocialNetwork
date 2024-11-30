using System.Text.RegularExpressions;
using Back.Models;
using Npgsql;

namespace Back.Services;

public class UserService
{
    private readonly HashSet<string> _loggedInUsers = new();

    private static DatabaseService _databaseService = DatabaseService.GetInstance();

    public string SignUp(string username, string email, string password, string firstName,
        string lastName, string phoneNumber, string? middleName = null)
    {
        ValidateSignUpData(username, email, password, firstName, middleName, lastName, phoneNumber);

        string joinDate = DateTime.Now.ToString("yyyy-MM-dd");
        decimal accessFee = 0; // default value
        string accountStatus = "active";
        string accountLevel = "user";

        User user = new(username, email, password, firstName, lastName, phoneNumber,
            accessFee, accountStatus, accountLevel, middleName);

        if (!IsUnique(user)) return "User already exists";

        password = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine("Hashed password: " + password);

        var query = @"
        INSERT INTO api_schema.""user"" (username, email, user_password, first_name, middle_name, last_name, phone_number, 
        join_date, access_fee, account_status, account_level) 
        VALUES (@username, @email, @password, @firstName, @middleName, @lastName, @phoneNumber, 
        @joinDate, @accessFee, @accountStatus, @accountLevel)";

        var parameters = new Dictionary<string, object>
        {
            { "@username", username },
            { "@email", email },
            { "@password", password },
            { "@firstName", firstName },
            { "@middleName", middleName ?? (object)DBNull.Value },
            { "@lastName", lastName },
            { "@phoneNumber", phoneNumber },
            { "@joinDate", joinDate },
            { "@accessFee", accessFee },
            { "@accountStatus", accountStatus },
            { "@accountLevel", accountLevel }
        };

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "Unexpected error occurred. Blame Volodymyr.";
        }

        return "";
    }

    private static bool IsUnique(User user)
    {
        string query = $"SELECT * FROM api_schema.user WHERE username = '{user.Username}' " +
                       " OR email = '{user.Email}' OR phone_number = '{user.PhoneNumber}'";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            using var reader = _databaseService.ExecuteQuery(query, out connection, out command);
            return !reader.HasRows;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    private void ValidateSignUpData(string username, string email, string password, string firstName,
        string? middleName,
        string lastName, string phoneNumber)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phoneNumber))
        {
            throw new ArgumentException("All fields are required.");
        }

        var usernameRegex = new Regex("^[a-zA-Z0-9_]{2,50}$");
        if (!usernameRegex.IsMatch(username))
        {
            // 2 characters for names like 'Li'
            throw new ArgumentException("Username must be between 2 and 50 characters long and contain only letters," +
                                        " numbers, and underscores.");
        }

        var emailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        if (!emailRegex.IsMatch(email))
        {
            throw new ArgumentException("Email is not in the correct format.");
        }

        var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$");
        if (!passwordRegex.IsMatch(password))
        {
            throw new ArgumentException(
                "Password must be at least 8 characters long" +
                " and include a mix of upper and lower case letters and numbers.");
        }

        var nameRegex = new Regex(@"^[a-zA-Z]{1,50}$");
        if (!nameRegex.IsMatch(firstName) || !nameRegex.IsMatch(lastName))
        {
            if (middleName != null && !nameRegex.IsMatch(middleName))
            {
                throw new ArgumentException("First, middle and last names must be between 1 and 50 characters long" +
                                            " and contain only letters.");
            }

            throw new ArgumentException("First, middle and last names must be between 1 and 50 characters long" +
                                        " and contain only letters.");
        }

        var phoneNumberRegex = new Regex(@"^\+?[0-9]{6,25}$");
        if (!phoneNumberRegex.IsMatch(phoneNumber))
        {
            throw new ArgumentException("Invalid phone number format. Example: +48123123123");
        }
    }

    public bool Login(string username, string password)
    {
        string query = "SELECT user_password FROM api_schema.user WHERE username = @Username";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Username", username }
            };

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (!reader.HasRows) return false;

            reader.Read();
            string storedPassword = reader.GetString(0);
            if (BCrypt.Net.BCrypt.Verify(password, storedPassword.Trim()))
            {
                _loggedInUsers.Add(username);
                return true;
            }

            return false;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public bool Logout(string username)
    {
        return _loggedInUsers.Remove(username);
    }

    public bool IsLoggedIn(string username)
    {
        return _loggedInUsers.Contains(username);
    }

    public static User GetUser(string username)
    {
        if (string.IsNullOrEmpty(username)) return null;
        string query = "SELECT * FROM api_schema.user WHERE username = @username";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@username", username }
            };

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (!reader.HasRows) return null;

            reader.Read();

            string? middleName = reader.IsDBNull(5) ? null : reader.GetString(5);

            return new User(
                reader.GetString(1), // username
                reader.GetString(2), // email
                reader.GetString(3), // user_password
                reader.GetString(4), // first_name
                reader.GetString(6), // last_name
                reader.GetString(7), // phone_number
                reader.GetDecimal(11), // access_fee
                reader.GetString(9), // account_status
                reader.GetString(10), // account_level
                middleName // middle_name
            );
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public static User GetUser(int id)
    {
        string query = "SELECT * FROM api_schema.\"user\" WHERE id = @id";
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@id", id }
            };

            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (!reader.HasRows) return null;

            reader.Read();

            string? middleName = reader.IsDBNull(5) ? null : reader.GetString(5);

            return new User(
                reader.GetString(1), // username
                reader.GetString(2), // email
                reader.GetString(3), // user_password
                reader.GetString(4), // first_name
                reader.GetString(6), // last_name
                reader.GetString(7), // phone_number
                reader.GetDecimal(11), // access_fee
                reader.GetString(9), // account_status
                reader.GetString(10), // account_level
                middleName // middle_name
            );
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }
}