namespace Back.Models;

public class User(string username, string email, string password, string firstName, string lastName,
    string phoneNumber, float accessFee, string accountStatus, string accountLevel)
{
    public string Username { get; set; } = username;
    public string Email { get; set; } = email;
    public string Password { get; set; } = password;
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
    public string PhoneNumber { get; set; } = phoneNumber;
    public DateTime JoinDate { get; set; } = DateTime.Now;
    public DateTime FreezeDate { get; set; } = DateTime.MinValue;
    public string AccountStatus { get; set; } = accountStatus;
    public string AccountLevel { get; set; } = accountLevel;
    public bool IsLoggedIn { get; set; } = false;
    public string Token { get; set; } = "";
    public DateTime LastLoginTime { get; set; } = DateTime.MinValue;
    public string LastLoginIP { get; set; } = "";

    
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
}