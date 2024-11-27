using System.Text.RegularExpressions;
using Back.Models;
using Npgsql;

namespace Back.Services;

public class UserService
{
    private readonly HashSet<string> _loggedInUsers = new();

    private static DatabaseService _databaseService = DatabaseService.GetInstance();

    public string SignUp(string username, string email, string password, string firstName, string middleName,
        string lastName, string phoneNumber)
    {
        ValidateSignUpData(username, email, password, firstName, middleName, lastName, phoneNumber);

        // CREATE TYPE api_schema.account_level AS ENUM ('user', 'admin');
        // CREATE TYPE api_schema.account_status AS ENUM ('active', 'frozen');
        // CREATE TABLE api_schema."user" (
        //     id SERIAL PRIMARY KEY,
        //     username VARCHAR(50) NOT NULL UNIQUE,
        //     email VARCHAR(50) NOT NULL UNIQUE,
        //     user_password CHAR(64) NOT NULL, -- SHA-256 hash
        //     first_name VARCHAR(50) NOT NULL,
        //     middle_name VARCHAR(50) NOT NULL
        //     last_name VARCHAR(50) NOT NULL,
        //     phone_number VARCHAR(25) NOT NULL UNIQUE,
        //     join_date DATE NOT NULL,
        //     account_status account_status NOT NULL,
        //     account_level account_level NOT NULL,
        //     access_fee INTEGER NOT NULL
        //     );

        string joinDate = DateTime.Now.ToString("yyyy-MM-dd");
        float accessFee = 0.0f; // 0 means not set
        string accountStatus = "active";
        string accountLevel = "user";

        User user = new(username, email, password, firstName, middleName, lastName, phoneNumber,
            accessFee, accountStatus, accountLevel);

        if (!IsUnique(user)) return "User already exists";

        password = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine("Hashed password: " + password);

        var query = $"INSERT INTO api_schema.\"user\" (username, email, user_password, first_name, last_name, phone_number, " +
                    "join_date, access_fee, account_status, account_level) " +
                    $"VALUES ('{username}', '{email}', '{password}', '{firstName}', '{lastName}', " +
                    $"'{phoneNumber}', '{joinDate}', {accessFee}, '{accountStatus}', '{accountLevel}')";
        
        try
        {
            _databaseService.ExecuteNonQuery(query);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "Unexpected error occurred. Blame Volodymyr.";
        }

        return "";
    }

    private bool IsUnique(User user)
    {
        string query = $"SELECT * FROM api_schema.user WHERE username = '{user.Username}' " +
                       " OR email = '{user.Email}' OR phone_number = '{user.PhoneNumber}'";
        using var reader = _databaseService.ExecuteQuery(query);
        return !reader.HasRows;
    }

    private void ValidateSignUpData(string username, string email, string password, string firstName, string middleName,
        string lastName, string phoneNumber)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phoneNumber))
        {
            throw new ArgumentException("All fields are required.");
        }

        var usernameRegex = new Regex("^[a-zA-Z0-9_]{2,50}$");
        if (!usernameRegex.IsMatch(username))
        { // 2 characters for names like 'Li'
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
        if (!nameRegex.IsMatch(firstName) || !nameRegex.IsMatch(middleName) || !nameRegex.IsMatch(lastName))
        {
            throw new ArgumentException("First and last names must be between 1 and 50 characters long" +
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
        var connection = _databaseService.GetConnection();
        
        string query = $"SELECT \"user_password\" FROM api_schema.\"user\" WHERE \"username\" = @Username";
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);
        using var reader = command.ExecuteReader();

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

    public bool Logout(string username)
    {
        return _loggedInUsers.Remove(username);
    }

    public bool IsLoggedIn(string username)
    {
        return _loggedInUsers.Contains(username);
    }
    
    public User? GetUser(string username)
    {
        if (string.IsNullOrEmpty(username)) return null;
        string query = $"SELECT * FROM api_schema.\"user\" WHERE username = '{username}'";
        using var reader = _databaseService.ExecuteQuery(query);
        if (!reader.HasRows) return null;
        
        reader.Read();
        
        return new User(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
            reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetFloat(7), reader.GetString(8), reader.GetString(9));
    }
    
}