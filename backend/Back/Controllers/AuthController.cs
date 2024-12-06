using Back.Services;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!_userService.Login(request.Username, request.Password))
        {
            return Unauthorized("Invalid username or password");
        }

        var token = _authService.GenerateToken(request.Username);
        return Ok(token ?? "Token generation failed");
    }

    [HttpPost("signup")]
    public IActionResult SignUp([FromBody] SignUpRequest request)
    {
        try
        {
            var image = "default.jpg";
            var isSignedUp = _userService.SignUp(request.Username, request.Email, request.Password,
                request.FirstName, request.LastName, request.PhoneNumber, image);

            if (isSignedUp != "") return BadRequest("User already exists");
            var token = _authService.GenerateToken(request.Username);
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
        string phoneNumber)
    {
        public string Username { get; set; } = username ?? throw new ArgumentNullException(nameof(username));
        public string Email { get; set; } = email ?? throw new ArgumentNullException(nameof(email));
        public string Password { get; set; } = password ?? throw new ArgumentNullException(nameof(password));
        public string FirstName { get; set; } = firstName ?? throw new ArgumentNullException(nameof(firstName));
        public string LastName { get; set; } = lastName ?? throw new ArgumentNullException(nameof(lastName));
        public string PhoneNumber { get; set; } = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
    }
}