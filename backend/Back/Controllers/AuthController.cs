using Back.Services;
using Microsoft.AspNetCore.Mvc;


namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(AuthService authService, UserService userService) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!userService.Login(request.Username, request.Password))
        {
            return Unauthorized("Invalid username or password");
        }

        var token = authService.GenerateToken(request.Username);
        return Ok(token ?? "Token generation failed");
    }

    [HttpPost("signup")]
    public IActionResult SignUp([FromBody] SignUpRequest request)
    {
        try
        {
            var image = "default.jpg";
            var isSignedUp = userService.SignUp(request.Username, request.Email, request.Password,
                request.FirstName, request.LastName, request.PhoneNumber, request.Description, image);

            if (isSignedUp != "") return BadRequest("User already exists");
            var token = authService.GenerateToken(request.Username);
            return Ok(token);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    public class LoginRequest(string username, string password)
    {
        public string Username { get; set; } = username ?? throw new ArgumentNullException(nameof(username));
        public string Password { get; set; } = password ?? throw new ArgumentNullException(nameof(password));
    }

    public class SignUpRequest(
        string username,
        string email,
        string password,
        string firstName,
        string lastName,
        string phoneNumber,
        string description = "")
    {
        public string Username { get; set; } = username ?? throw new ArgumentNullException(nameof(username));
        public string Email { get; set; } = email ?? throw new ArgumentNullException(nameof(email));
        public string Password { get; set; } = password ?? throw new ArgumentNullException(nameof(password));
        public string FirstName { get; set; } = firstName ?? throw new ArgumentNullException(nameof(firstName));
        public string LastName { get; set; } = lastName ?? throw new ArgumentNullException(nameof(lastName));
        public string PhoneNumber { get; set; } = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));

        public string Description { get; set; } = description;
    }
}