using System.Data.Common;
using System.Text.RegularExpressions;
using Back.Models;
using Npgsql;
using Back.Services.Interfaces;

namespace Back.Services;

public class UserService : IUserService
{
    private static class ValidationPatterns
    {
        public const string Username = @"^[\w\-_]{2,50}$";  // Keep username simple for URLs
        public const string Email = @"^[\w\-\.]+@([\w\-]+\.)+[\w\-]{2,4}$";
        public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$";
        // Updated pattern for names to support extended characters
        public const string Name = @"^[\p{L}\p{M}]{1,50}$";  // Unicode letter or mark
        public const string Phone = @"^\+?[0-9]{6,25}$";
    }

    private static class SqlQueries
    {
        public const string GetUserProfile = @"
            WITH UserRanking AS (
                SELECT user_id, ROW_NUMBER() OVER (ORDER BY total_likes DESC) as rating
                FROM api_schema.user_rating
            )
            SELECT 
                u.username, u.first_name, u.last_name, u.profile_description, 
                u.profile_picture, w.amount,
                (SELECT COALESCE(SUM(likes), 0) FROM api_schema.post WHERE user_id = u.id) as total_likes,
                (SELECT COUNT(*) FROM api_schema.chat 
                 WHERE (buyer_id = u.id OR seller_id = u.id) 
                 AND chat_status = 'disabled') as completed_tasks,
                COALESCE(r.rating, 0) as rating
            FROM api_schema.user u
            LEFT JOIN api_schema.wallet w ON u.id = w.user_id
            LEFT JOIN UserRanking r ON r.user_id = u.id
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
    private readonly IImageService _imageService; // Add this field
    private readonly IAuthService _authService; // Add this field

    public UserService(IDatabaseService databaseService, IImageService imageService, IAuthService authService)
    {
        _databaseService = databaseService;
        _imageService = imageService;
        _authService = authService;
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
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            ValidateUserData(username, email, password, firstName, lastName, phoneNumber);

            var user = new User(username, email, password, firstName, lastName, phoneNumber,
                0, AccountTypes.Status.active.ToString(), AccountTypes.Level.user.ToString(), 
                "Użytkownik nie dodał jeszcze opisu.", 
                null);

            string uniqueErrorMessage;
            if (!IsUnique(user, out uniqueErrorMessage))
            {
                return uniqueErrorMessage;
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var joinDate = DateTime.Now.Date;

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

            using var command = new NpgsqlCommand(userQuery, connection);
            
            // Add parameters with explicit DbType for nullable values
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@password", hashedPassword);
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@phoneNumber", phoneNumber);
            command.Parameters.AddWithValue("@joinDate", joinDate);
            command.Parameters.AddWithValue("@accessFee", 0);
            command.Parameters.AddWithValue("@accountStatus", AccountTypes.Status.active.ToString());
            command.Parameters.AddWithValue("@accountLevel", AccountTypes.Level.user.ToString());
            command.Parameters.AddWithValue("@description", "Użytkownik nie dodał jeszcze opisu.");
            command.Parameters.AddWithValue("@profileImage", DBNull.Value);

            var userId = (int)command.ExecuteScalar();

            // Create wallet for the user
            var walletQuery = @"
                INSERT INTO api_schema.wallet (amount, user_id)
                VALUES (@amount, @userId)";

            using var walletCommand = new NpgsqlCommand(walletQuery, connection);
            walletCommand.Parameters.AddWithValue("@amount", 0);
            walletCommand.Parameters.AddWithValue("@userId", userId);
            walletCommand.ExecuteNonQuery();

            transaction.Commit();
            return "";
        }
        catch (PostgresException ex)
        {
            transaction.Rollback();
            Console.WriteLine($"Database error: {ex.MessageText}");
            return $"Database error: {ex.MessageText}";
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return $"An unexpected error occurred during registration. {ex.Message}";
        }
    }

    public async Task<bool> Login(string username, string password)
    {
        string query = "SELECT user_password, account_status FROM api_schema.user WHERE username = @Username";
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
            string accountStatus = reader.GetString(1);

            if (accountStatus == "frozen")
            {
                throw new UnauthorizedAccessException("Account is frozen");
            }

            if (BCrypt.Net.BCrypt.Verify(password, storedPassword.Trim()))
            {
                // Generate token asynchronously
                var token = await _authService.GenerateToken(username);
                if (token == null)
                {
                    return false;
                }

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
                reader.GetStringOrDefault(reader.GetOrdinal("profile_picture"))  // Removed default value
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
                reader.GetStringOrDefault(reader.GetOrdinal("profile_description")),
                reader.GetStringOrDefault(reader.GetOrdinal("profile_picture")),
                includeWallet ? (reader.IsDBNull(reader.GetOrdinal("amount")) ? 0 : reader.GetInt32(reader.GetOrdinal("amount"))) : 0,
                reader.GetInt32(reader.GetOrdinal("total_likes")),
                reader.GetInt32(reader.GetOrdinal("completed_tasks")),
                reader.GetInt32(reader.GetOrdinal("rating"))  // Added rating
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

    public UserProfile GetProfile(string username)
    {
        var sql = @"
            WITH UserRanking AS (
                SELECT user_id, ROW_NUMBER() OVER (ORDER BY total_likes DESC) as rating
                FROM api_schema.user_rating
            )
            SELECT 
                u.username,
                u.first_name,
                u.last_name,
                u.profile_description,
                u.profile_picture,
                COALESCE(ur.total_likes, 0) as total_likes,
                (SELECT COUNT(*) FROM api_schema.post p WHERE p.user_id = u.id) as completed_tasks,
                COALESCE(r.rating, 0) as rating
            FROM api_schema.""user"" u
            LEFT JOIN api_schema.user_rating ur ON ur.user_id = u.id
            LEFT JOIN UserRanking r ON r.user_id = u.id
            WHERE u.username = @Username";

        using var reader = _databaseService.ExecuteQuery(sql, out var connection, out var command,
            new Dictionary<string, object> { { "@Username", username } });
        try
        {
            if (!reader.HasRows) return null;

            reader.Read();
            return new UserProfile(
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("first_name")),
                reader.GetString(reader.GetOrdinal("last_name")),
                reader.GetStringOrDefault(reader.GetOrdinal("profile_description")),
                reader.GetStringOrDefault(reader.GetOrdinal("profile_picture")),
                null, // rubies
                reader.GetInt32(reader.GetOrdinal("total_likes")),
                reader.GetInt32(reader.GetOrdinal("completed_tasks")),
                reader.GetInt32(reader.GetOrdinal("rating"))  // Added rating
            );
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public User.EditDataResponse EditData(string username)
    {
        var query = @"
            SELECT username, email, phone_number, profile_description, access_fee
            FROM api_schema.user
            WHERE username = @Username";

        using var reader = _databaseService.ExecuteQuery(query, out var connection, out var command, 
            new Dictionary<string, object> { { "@Username", username } });
        try
        {
            if (!reader.HasRows) return null;

            reader.Read();
            return new User.EditDataResponse
            {
                Description = reader.GetStringOrDefault(reader.GetOrdinal("profile_description")),
                Username = reader.GetString(reader.GetOrdinal("username")),
                PhoneNumber = reader.GetString(reader.GetOrdinal("phone_number")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                AccessFee = reader.GetInt32(reader.GetOrdinal("access_fee"))
            };
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public async Task<bool> EditBasicProfile(string username, User.EditBasicRequest request)
    {
        var parameters = new Dictionary<string, object>();
        var setStatements = new List<string>();

        if (!string.IsNullOrEmpty(request.FirstName))
        {
            if (!Regex.IsMatch(request.FirstName, ValidationPatterns.Name))
                throw new ArgumentException("Invalid first name format");
                
            setStatements.Add("first_name = @firstName");
            parameters.Add("@firstName", request.FirstName);
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            if (!Regex.IsMatch(request.LastName, ValidationPatterns.Name))
                throw new ArgumentException("Invalid last name format");
                
            setStatements.Add("last_name = @lastName");
            parameters.Add("@lastName", request.LastName);
        }

        if (!string.IsNullOrEmpty(request.Description))
        {
            setStatements.Add("profile_description = @description");
            parameters.Add("@description", request.Description);
        }

        if (request.ProfileImage != null)
        {
            var imagePath = await _imageService.UploadImageAsync(request.ProfileImage, username);
            setStatements.Add("profile_picture = @profilePicture");
            parameters.Add("@profilePicture", imagePath);
        }

        if (!setStatements.Any())
            return true; // Nothing to update

        parameters.Add("@username", username);

        var query = $@"
            UPDATE api_schema.user 
            SET {string.Join(", ", setStatements)}
            WHERE username = @username";

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> EditSensitiveProfile(string username, User.EditSensitiveRequest request)
    {
        // First verify the current password
        var user = GetUser(username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
            return false;

        var parameters = new Dictionary<string, object>();
        var setStatements = new List<string>();

        if (!string.IsNullOrEmpty(request.NewUsername))
        {
            if (!Regex.IsMatch(request.NewUsername, ValidationPatterns.Username))
                throw new ArgumentException("Invalid username format");
                
            setStatements.Add("username = @newUsername");
            parameters.Add("@newUsername", request.NewUsername);
        }

        if (!string.IsNullOrEmpty(request.NewEmail))
        {
            if (!Regex.IsMatch(request.NewEmail, ValidationPatterns.Email))
                throw new ArgumentException("Invalid email format");
                
            setStatements.Add("email = @newEmail");
            parameters.Add("@newEmail", request.NewEmail);
        }

        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (!Regex.IsMatch(request.NewPassword, ValidationPatterns.Password))
                throw new ArgumentException("Invalid password format");
                
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            setStatements.Add("user_password = @newPassword");
            parameters.Add("@newPassword", hashedPassword);
        }

        if (!string.IsNullOrEmpty(request.NewPhoneNumber))
        {
            if (!Regex.IsMatch(request.NewPhoneNumber, ValidationPatterns.Phone))
                throw new ArgumentException("Invalid phone number format");
                
            setStatements.Add("phone_number = @newPhoneNumber");
            parameters.Add("@newPhoneNumber", request.NewPhoneNumber);
        }

        if (request.NewAccessFee.HasValue)
        {
            setStatements.Add("access_fee = @newAccessFee");
            parameters.Add("@newAccessFee", request.NewAccessFee.Value);
        }

        if (!setStatements.Any())
            return true; // Nothing to update

        parameters.Add("@username", username);

        var query = $@"
            UPDATE api_schema.user 
            SET {string.Join(", ", setStatements)}
            WHERE username = @username";

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

// Extension method for DbDataReader
public static class DbDataReaderExtensions
{
    public static string GetStringOrDefault(this DbDataReader reader, int ordinal, string defaultValue = "") =>
        reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
}