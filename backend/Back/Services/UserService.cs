using System.Text.RegularExpressions;
using Back.Models;
using Npgsql;
using Back.Services.Interfaces;

namespace Back.Services;

public class UserService : IUserService
{
    private static class ValidationPatterns
    {
        public const string Username = @"^[a-zA-Z0-9_]{2,50}$";
        public const string Email = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$";
        public const string Name = @"^[a-zA-Z]{1,50}$";
        public const string Phone = @"^\+?[0-9]{6,25}$";
    }

    private static class SqlQueries
    {
        public const string GetUserProfile = @"
            SELECT u.username, u.first_name, u.last_name, u.profile_description, u.profile_picture, w.amount
            FROM api_schema.user u
            LEFT JOIN api_schema.wallet w ON u.id = w.user_id
            WHERE u.username = @username";

        public const string GetUserRatings = @"
            SELECT t.tag_name, r.rating
            FROM api_schema.user_rating r
            JOIN api_schema.rating_list rl ON r.list_id = rl.id
            JOIN api_schema.tags t ON rl.tag_id = t.id
            WHERE r.user_id = (SELECT id FROM api_schema.user WHERE username = @username)";

        public const string UpdateUser = @"
            UPDATE api_schema.user 
            SET email = @Email, first_name = @FirstName,
                last_name = @LastName, phone_number = @PhoneNumber, 
                profile_description = @Description,
                profile_picture = @ProfileImage, 
                access_fee = @AccessFee
                {0}
            WHERE username = @Username";

        public const string GetUserByUsername = @"
            SELECT * FROM api_schema.user WHERE username = @username";
    }

    private static class AccountTypes
    {
        public enum Status
        {
            active,
            frozen
        }

        public enum Level
        {
            user,
            admin
        }
    }

    private readonly HashSet<string> _loggedInUsers = new();
    private readonly IDatabaseService _databaseService;

    public UserService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    private void ValidateUserData(string username, string email, string password, string firstName,
        string lastName, string phoneNumber, bool validateUsername = true)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) ||
            string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phoneNumber))
        {
            throw new ArgumentException("All fields are required.");
        }

        if (validateUsername)
        {
            if (string.IsNullOrEmpty(username) || !Regex.IsMatch(username, ValidationPatterns.Username))
            {
                throw new ArgumentException("Username must be between 2 and 50 characters long and contain only letters, numbers, and underscores.");
            }
        }

        if (!Regex.IsMatch(email, ValidationPatterns.Email))
            throw new ArgumentException("Email is not in the correct format.");

        if (!string.IsNullOrEmpty(password) && !Regex.IsMatch(password, ValidationPatterns.Password))
            throw new ArgumentException("Password must be at least 8 characters long and include a mix of upper and lower case letters and numbers.");

        if (!Regex.IsMatch(firstName, ValidationPatterns.Name) || !Regex.IsMatch(lastName, ValidationPatterns.Name))
            throw new ArgumentException("Names must be between 1 and 50 characters long and contain only letters.");

        if (!Regex.IsMatch(phoneNumber, ValidationPatterns.Phone))
            throw new ArgumentException("Invalid phone number format. Example: +48123123123");
    }

    private bool IsUnique(User user, out string errorMessage)
    {
        errorMessage = "";
        try
        {
            var checks = new[]
            {
                (query: "SELECT COUNT(*) FROM api_schema.user WHERE username = @Username", 
                 message: "Username already taken"),
                (query: "SELECT COUNT(*) FROM api_schema.user WHERE email = @Email", 
                 message: "Email already registered"),
                (query: "SELECT COUNT(*) FROM api_schema.user WHERE phone_number = @PhoneNumber", 
                 message: "Phone number already registered")
            };

            foreach (var (query, message) in checks)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "@Username", user.Username },
                    { "@Email", user.Email },
                    { "@PhoneNumber", user.PhoneNumber }
                };

                using var reader = _databaseService.ExecuteQuery(query, out var connection, out var command, parameters);
                try
                {
                    if (reader.Read() && reader.GetInt32(0) > 0)
                    {
                        errorMessage = message;
                        return false;
                    }
                }
                finally
                {
                    command?.Dispose();
                    connection?.Dispose();
                }
            }
            return true;
        }
        catch (Exception)
        {
            errorMessage = "Database error occurred while checking user existence";
            return false;
        }
    }

    public string SignUp(string username, string email, string password, string firstName,
        string lastName, string phoneNumber, string profileImage)
    {
        try
        {
            ValidateUserData(username, email, password, firstName, lastName, phoneNumber);

            var user = new User(username, email, password, firstName, lastName, phoneNumber,
                0, AccountTypes.Status.active.ToString(), AccountTypes.Level.user.ToString(), 
                "Użytkownik nie dodał jeszcze opisu.", 
                string.IsNullOrEmpty(profileImage) ? "default.jpg" : profileImage);

            string uniqueErrorMessage;
            if (!IsUnique(user, out uniqueErrorMessage))
            {
                return uniqueErrorMessage;
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var joinDate = DateTime.Now.Date;

            // Create an image container for the user's profile
            var createContainerQuery = @"
                INSERT INTO api_schema.image_container (amount_of_images)
                VALUES (0) RETURNING id";

            var containerId = 0;
            using (var connection = new NpgsqlConnection(_databaseService.GetConnection().ConnectionString))
            {
                connection.Open();
                using var command = new NpgsqlCommand(createContainerQuery, connection);
                containerId = (int)command.ExecuteScalar();
            }

            // Insert user with all required fields
            var userQuery = @"
                INSERT INTO api_schema.user (
                    username, email, user_password, first_name, last_name, phone_number,
                    join_date, access_fee, account_status, account_level, profile_description, 
                    profile_picture
                ) VALUES (
                    @username, @email, @password, @firstName, @lastName, @phoneNumber,
                    @joinDate, @accessFee, 
                    cast(@accountStatus as api_schema.account_status), 
                    cast(@accountLevel as api_schema.account_level), 
                    @description, @profileImage
                ) RETURNING id";

            var parameters = new Dictionary<string, object>
            {
                { "@username", username },
                { "@email", email },
                { "@password", hashedPassword },
                { "@firstName", firstName },
                { "@lastName", lastName },
                { "@phoneNumber", phoneNumber },
                { "@joinDate", joinDate },
                { "@accessFee", 0 },
                { "@accountStatus", AccountTypes.Status.active.ToString() },
                { "@accountLevel", AccountTypes.Level.user.ToString() },
                { "@description", "Użytkownik nie dodał jeszcze opisu." },
                { "@profileImage", user.ProfileImage }
            };

            var userId = 0;
            using (var connection = new NpgsqlConnection(_databaseService.GetConnection().ConnectionString))
            {
                connection.Open();
                using var command = new NpgsqlCommand(userQuery, connection);
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
                userId = (int)command.ExecuteScalar();
            }

            // Create wallet for the user
            var walletQuery = @"
                INSERT INTO api_schema.wallet (amount, user_id)
                VALUES (@amount, @userId)";

            var walletParameters = new Dictionary<string, object>
            {
                { "@amount", 0 },
                { "@userId", userId }
            };

            _databaseService.ExecuteNonQuery(walletQuery, walletParameters);

            // Create initial user ratings
            var ratingQuery = @"
                INSERT INTO api_schema.user_rating (user_id, list_id, rating)
                SELECT @userId, id, 0 FROM api_schema.rating_list";

            var ratingParameters = new Dictionary<string, object>
            {
                { "@userId", userId }
            };

            _databaseService.ExecuteNonQuery(ratingQuery, ratingParameters);
            return "";
        }
        catch (ArgumentException ex)
        {
            return $"Validation error: {ex.Message}";
        }
        catch (PostgresException ex)
        {
            switch (ex.SqlState)
            {
                case "23505": // unique_violation
                    return "This username, email, or phone number is already registered.";
                case "23514": // check_violation
                    return "Invalid data format provided.";
                case "42804": // datatype_mismatch
                    return "Internal error: Invalid data type for account status or level.";
                default:
                    return $"Database error: {ex.MessageText}";
            }
        }
        catch (Exception)
        {
            return "An unexpected error occurred during registration.";
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

    private User ExecuteUserQuery(string query, Dictionary<string, object> parameters)
    {
        using var reader = _databaseService.ExecuteQuery(query, out var connection, out var command, parameters);
        try
        {
            if (!reader.HasRows) return null;
            reader.Read();

            return new User(
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("user_password")),
                reader.GetString(reader.GetOrdinal("first_name")),
                reader.GetString(reader.GetOrdinal("last_name")),
                reader.GetString(reader.GetOrdinal("phone_number")),
                reader.GetDecimal(reader.GetOrdinal("access_fee")),
                reader.GetString(reader.GetOrdinal("account_status")),
                reader.GetString(reader.GetOrdinal("account_level")),
                reader.GetStringOrDefault(reader.GetOrdinal("profile_description")),
                reader.GetStringOrDefault(reader.GetOrdinal("profile_picture"), "default.png")
            );
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    private Dictionary<string, int> GetUserRatings(string username)
    {
        var ratings = new Dictionary<string, int>();
        var parameters = new Dictionary<string, object> { { "@username", username } };

        using var reader = _databaseService.ExecuteQuery(SqlQueries.GetUserRatings, out var connection, out var command, parameters);
        try
        {
            while (reader.Read())
            {
                ratings.Add(reader.GetString(0), reader.GetInt32(1));
            }
            return ratings;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public User GetUser(string username) => 
        ExecuteUserQuery(SqlQueries.GetUserByUsername, new Dictionary<string, object> { { "@username", username } });

    public User GetUser(int id) =>
        ExecuteUserQuery("SELECT * FROM api_schema.user WHERE id = @id", new Dictionary<string, object> { { "@id", id } });

    public UserProfile GetProfile(string username, bool includeWallet = false)
    {
        var parameters = new Dictionary<string, object> { { "@username", username } };

        using var reader = _databaseService.ExecuteQuery(SqlQueries.GetUserProfile, out var connection, out var command, parameters);
        try
        {
            if (!reader.HasRows) return null;

            reader.Read();
            var profile = new UserProfile(
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("first_name")),
                reader.GetString(reader.GetOrdinal("last_name")),
                GetUserRatings(username),
                reader.GetStringOrDefault(reader.GetOrdinal("profile_description")),
                reader.GetStringOrDefault(reader.GetOrdinal("profile_picture")),
                includeWallet ? (reader.IsDBNull(reader.GetOrdinal("amount")) ? 0 : reader.GetInt32(reader.GetOrdinal("amount"))) : 0
            );

            return profile;
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public UserProfile GetOwnProfile(string username) => GetProfile(username, true);

    public UserProfile GetProfile(string username) => GetProfile(username, false);

    public User.EditRequest EditData(string username)
    {
        var query = @"
            SELECT email, first_name, last_name, phone_number, profile_description, 
                   profile_picture, access_fee
            FROM api_schema.user
            WHERE username = @Username";

        using var reader = _databaseService.ExecuteQuery(query, out var connection, out var command, 
            new Dictionary<string, object> { { "@Username", username } });
        try
        {
            if (!reader.HasRows) return null;

            reader.Read();
            return new User.EditRequest
            {
                Email = reader.GetString(0),
                Password = "",
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                PhoneNumber = reader.GetString(3),
                Description = reader.GetStringOrDefault(4),
                ProfileImage = reader.GetStringOrDefault(5),
                AccessFee = reader.GetInt32(6)
            };
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public bool EditProfile(string username, User.EditRequest request)
    {
        ValidateUserData(null, request.Email, request.Password, request.FirstName, 
            request.LastName, request.PhoneNumber, false);

        var passwordClause = !string.IsNullOrEmpty(request.Password) 
            ? ", user_password = @Password" 
            : "";
        
        var query = string.Format(SqlQueries.UpdateUser, passwordClause);
        var parameters = new Dictionary<string, object>
        {
            { "@Email", request.Email },
            { "@FirstName", request.FirstName },
            { "@LastName", request.LastName },
            { "@PhoneNumber", request.PhoneNumber },
            { "@Description", request.Description },
            { "@ProfileImage", request.ProfileImage },
            { "@AccessFee", request.AccessFee },
            { "@Username", username }
        };

        if (!string.IsNullOrEmpty(request.Password))
        {
            parameters.Add("@Password", BCrypt.Net.BCrypt.HashPassword(request.Password));
        }

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
}

// Extension method for NpgsqlDataReader
public static class NpgsqlDataReaderExtensions
{
    public static string GetStringOrDefault(this NpgsqlDataReader reader, int ordinal, string defaultValue = "") =>
        reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
}