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
}