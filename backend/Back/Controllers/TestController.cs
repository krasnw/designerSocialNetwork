using Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController(AuthService authService, UserService userService) : ControllerBase
    {
        [Authorize]
        [HttpPost("test")]
        public IActionResult Test()
        {
            var user = User.Identity?.Name;
            return Ok(new { message = "Hello, " + user });
        }

        [HttpGet("token")]
        public string Token()
        {
            return authService.GenerateToken("TestUser") ?? "Token generation failed";
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!userService.IsSignedUp(request.Username) || !userService.Login(request.Username, request.Password))
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
                var isSignedUp = userService.SignUp(request.Username, request.Email, request.Password, request.FirstName,
                    request.LastName, request.PhoneNumber);
                if (!isSignedUp) return BadRequest("User already exists");
                return Ok("User signed up successfully");
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

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var username = User.Identity?.Name;
            if (username != null && userService.IsLoggedIn(username))
            {
                userService.Logout(username);
                return Ok("User logged out successfully");
            }

            return BadRequest("User is not logged in");
        }
    }

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