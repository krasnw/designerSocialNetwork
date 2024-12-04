using System.Text.RegularExpressions;
using Back.Models;
using Npgsql;

namespace Back.Services;

public class UserService
{
    private readonly HashSet<string> _loggedInUsers = new();

    private static DatabaseService _databaseService = DatabaseService.GetInstance();

    public string SignUp(string username, string email, string password, string firstName,
        string lastName, string phoneNumber, string profileImage)
    {
        ValidateSignUpData(username, email, password, firstName, lastName, phoneNumber);

        string joinDate = DateTime.Now.ToString("yyyy-MM-dd");
        decimal accessFee = 0; // default value
        string accountStatus = "active";
        string accountLevel = "user";
        var description = "Użytkownik nie dodał jeszcze opisu.";
        if (profileImage == null || profileImage == "") profileImage = "default.jpg";

        User user = new(username, email, password, firstName, lastName, phoneNumber,
            accessFee, accountStatus, accountLevel, description, profileImage);

        if (!IsUnique(user)) return "User already exists";

        password = BCrypt.Net.BCrypt.HashPassword(password);


        var query = @"
        INSERT INTO api_schema.""user"" (username, email, user_password, first_name, last_name, phone_number,
        join_date, access_fee, account_status, account_level, profile_description, profile_picture)
        VALUES (@username, @email, @password, @firstName, @lastName, @phoneNumber,
        @joinDate, @accessFee, @accountStatus, @accountLevel, @description, @profileImage)";

        var parameters = new Dictionary<string, object>
        {
            { "@username", username },
            { "@email", email },
            { "@password", password },
            { "@firstName", firstName },
            { "@lastName", lastName },
            { "@phoneNumber", phoneNumber },
            { "@joinDate", joinDate },
            { "@accessFee", accessFee },
            { "@accountStatus", accountStatus },
            { "@accountLevel", accountLevel },
            { "@description", description },
            { "@profileImage", profileImage }
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
            
            return new User(
                reader.GetString(1), // username
                reader.GetString(2), // email
                reader.GetString(3), // user_password
                reader.GetString(4), // first_name
                reader.GetString(5), // last_name
                reader.GetString(6), // phone_number
                reader.GetDecimal(12), // access_fee
                reader.GetString(10), // account_status
                reader.GetString(11), // account_level
                reader.IsDBNull(7) ? "" : reader.GetString(7), // profile_description
                reader.IsDBNull(8) ? "" : reader.GetString(8) // profile_picture
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
        string query = "SELECT * FROM api_schema.user WHERE id = @id";
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

            return new User(
                reader.GetString(reader.GetOrdinal("username")), // username
                reader.GetString(reader.GetOrdinal("email")), // email
                reader.GetString(reader.GetOrdinal("user_password")), // user_password
                reader.GetString(reader.GetOrdinal("first_name")), // first_name
                reader.GetString(reader.GetOrdinal("last_name")), // last_name
                reader.GetString(reader.GetOrdinal("phone_number")), // phone_number
                reader.GetDecimal(reader.GetOrdinal("access_fee")), // access_fee
                reader.GetString(reader.GetOrdinal("account_status")), // account_status
                reader.GetString(reader.GetOrdinal("account_level")), // account_level
                reader.IsDBNull(reader.GetOrdinal("profile_description"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("profile_description")), // profile_description
                reader.IsDBNull(reader.GetOrdinal("profile_picture"))
                    ? "default.png"
                    : reader.GetString(reader.GetOrdinal("profile_picture")) // profile_picture
            );
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public static UserProfile GetOwnProfile(string username)
    {
        var userQuery = @"
SELECT u.username, u.first_name, u.last_name, u.profile_description, u.profile_picture, w.amount
FROM api_schema.user u
JOIN api_schema.wallet w ON u.id = w.user_id
WHERE u.username = @username";

        var ratingQuery = @"
SELECT t.tag_name, r.rating
FROM api_schema.user_rating r
    JOIN api_schema.rating_list rl ON r.list_id = rl.id
JOIN api_schema.tags t ON rl.tag_id = t.id
WHERE r.user_id = (SELECT id FROM api_schema.user WHERE username = @username)";

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@username", username }
            };

            // Execute the first query to get user details
            using var userReader = _databaseService.ExecuteQuery(userQuery, out connection, out command, parameters);
            if (!userReader.HasRows) return null;

            userReader.Read();
            string firstName = userReader.GetString(1);
            string lastName = userReader.GetString(2);
            string description = userReader.IsDBNull(3) ? "" : userReader.GetString(3);
            string profileImage = userReader.IsDBNull(4) ? "" : userReader.GetString(4);
            int rubies = userReader.GetInt32(5);

            // Execute the second query to get rating positions
            var ratingPositions = new Dictionary<string, int>();
            using var ratingReader =
                _databaseService.ExecuteQuery(ratingQuery, out connection, out command, parameters);
            while (ratingReader.Read())
            {
                string tagName = ratingReader.GetString(0);
                int position = ratingReader.GetInt32(1);
                ratingPositions.Add(tagName, position);
            }

            return new UserProfile(username, firstName, lastName, ratingPositions, description, profileImage, rubies);
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public static UserProfile GetProfile(string username)
    {
        var userQuery = @"
SELECT u.username, u.first_name, u.last_name, u.profile_description, u.profile_picture
FROM api_schema.user u
WHERE u.username = @username";

        var ratingQuery = @"
SELECT t.tag_name, r.rating
FROM api_schema.user_rating r
    JOIN api_schema.rating_list rl ON r.list_id = rl.id
JOIN api_schema.tags t ON rl.tag_id = t.id
WHERE r.user_id = (SELECT id FROM api_schema.user WHERE username = @username)";

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@username", username }
            };

            // Execute the first query to get user details
            using var userReader = _databaseService.ExecuteQuery(userQuery, out connection, out command, parameters);
            if (!userReader.HasRows) return null;

            userReader.Read();
            string firstName = userReader.GetString(1);
            string lastName = userReader.GetString(2);
            string description = userReader.IsDBNull(3) ? "" : userReader.GetString(3);
            string profileImage = userReader.IsDBNull(4) ? "" : userReader.GetString(4);

            // Execute the second query to get rating positions
            var ratingPositions = new Dictionary<string, int>();
            using var ratingReader =
                _databaseService.ExecuteQuery(ratingQuery, out connection, out command, parameters);
            while (ratingReader.Read())
            {
                string tagName = ratingReader.GetString(0);
                int position = ratingReader.GetInt32(1);
                ratingPositions.Add(tagName, position);
            }

            return new UserProfile(username, firstName, lastName, ratingPositions, description, profileImage);
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public static User.EditRequest EditData(string username)
    {
        var query = @"
        SELECT email, first_name, last_name, phone_number, profile_description, profile_picture, access_fee
        FROM api_schema.user
        WHERE username = @Username";

        var parameters = new Dictionary<string, object>
        {
            { "@Username", username }
        };

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;

        try
        {
            using var reader = _databaseService.ExecuteQuery(query, out connection, out command, parameters);
            if (!reader.HasRows) return null;

            reader.Read();
            return new User.EditRequest
            {
                Email = reader.GetString(0),
                Password = "",
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                PhoneNumber = reader.GetString(3),
                Description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ProfileImage = reader.IsDBNull(5) ? "" : reader.GetString(5),
                AccessFee = reader.GetInt32(6)
            };
        }
        finally
        {
            command?.Dispose();
            connection?.Dispose();
        }
    }

    public static bool EditProfile(string username, User.EditRequest request)
    {
        if (!ValidateEditData(request)) return false;

        var query = @"
    UPDATE api_schema.user 
    SET email = @Email, first_name = @FirstName,
    last_name = @LastName, phone_number = @PhoneNumber, profile_description = @Description,
    profile_picture = @ProfileImage, access_fee = @AccessFee";

        if (!string.IsNullOrEmpty(request.Password))
        {
            query += ", user_password = @Password";
        }

        query += " WHERE username = @Username";

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

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;

        try
        {
            connection = _databaseService.GetConnection();

            command = new NpgsqlCommand(query, connection);
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }

            command.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        finally
        {
            command?.Dispose();
            connection?.Close();
            connection?.Dispose();
        }
    }

    private static bool ValidateEditData(User.EditRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.FirstName) ||
            string.IsNullOrEmpty(request.LastName) || string.IsNullOrEmpty(request.PhoneNumber))
        {
            throw new ArgumentException("All fields are required.");
        }

        var emailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        if (!emailRegex.IsMatch(request.Email))
        {
            throw new ArgumentException("Email is not in the correct format.");
        }

        var nameRegex = new Regex(@"^[a-zA-Z]{1,50}$");
        if (!nameRegex.IsMatch(request.FirstName) || !nameRegex.IsMatch(request.LastName))
        {
            throw new ArgumentException("First and last names must be between 1 and 50 characters long" +
                                        " and contain only letters.");
        }

        var phoneNumberRegex = new Regex(@"^\+?[0-9]{6,25}$");
        if (!phoneNumberRegex.IsMatch(request.PhoneNumber))
        {
            throw new ArgumentException("Invalid phone number format. Example: +48123123123");
        }

        return true;
    }
}