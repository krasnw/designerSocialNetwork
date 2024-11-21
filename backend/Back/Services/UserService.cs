using System.Text.RegularExpressions;
using Back.Models;
using Npgsql;

namespace Back.Services;

public class UserService
{
    private readonly HashSet<string> _loggedInUsers = new();

    private static DatabaseService _databaseService =
        DatabaseService.GetInstance("Host=localhost;Port=5433;Username=api_user;" +
                                    "Password=api_user_password;Database=api_database;SearchPath=api_schema;");

    public bool SignUp(string username, string email, string password, string firstName, string lastName,
        string phoneNumber)
    {
        ValidateSignUpData(username, email, password, firstName, lastName, phoneNumber);

        /*-- User block
        CREATE TYPE api_schema.account_level AS ENUM ('user', 'admin');
        CREATE TYPE api_schema.account_status AS ENUM ('active', 'frozen');
        CREATE TABLE api_schema."user" (
            id SERIAL PRIMARY KEY,
            username VARCHAR(50) NOT NULL,                                  | provided
            email VARCHAR(50) NOT NULL,                                     | provided
            user_password CHAR(64) NOT NULL, -- SHA-256 hash                | provided
            first_name VARCHAR(50) NOT NULL,                                | provided
            last_name VARCHAR(50) NOT NULL,                                 | provided
            phone_number VARCHAR(25) NOT NULL,                              | provided
            join_date DATE NOT NULL,                                        | generated
            freeze_date DATE,                                               | unset
            account_status account_status NOT NULL,                         | default 'active'
            account_level account_level NOT NULL,                           | default 'user'
            access_fee INTEGER NOT NULL                                     | must be unset by default
        );*/

        string joinDate = DateTime.Now.ToString("yyyy-MM-dd");
        float accessFee = 0.0f; // 0 means not set
        string accountStatus = "active";
        string accountLevel = "user";

        User user = new(username, email, password, firstName, lastName, phoneNumber,
            accessFee, accountStatus, accountLevel);

        if (!IsUnique(user)) return false;

        password = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine("Hashed password: " + password);
        //salted version of password: password = BCrypt.Net.BCrypt.HashPassword(password, 12);

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
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    private bool IsUnique(User user)
    {
        string query = $"SELECT * FROM api_schema.user WHERE username = '{user.Username}' " +
                       " OR email = '{user.Email}' OR phone_number = '{user.PhoneNumber}'";
        using var reader = _databaseService.ExecuteQuery(query);
        return !reader.HasRows;
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
        {// Do we need username validation?
            throw new ArgumentException("Username must be between 3 and 50 characters long and contain only letters," +
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
                "Password must be at least 8 characters long and include a mix of upper and lower case letters and numbers.");
        }

        var phoneNumberRegex = new Regex(@"^\+?[0-9]{6,25}$");
        if (!phoneNumberRegex.IsMatch(phoneNumber))
        {
            throw new ArgumentException("Invalid phone number format.");
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
        //if (password == storedPassword.Trim()) //TODO: Hash password
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
}