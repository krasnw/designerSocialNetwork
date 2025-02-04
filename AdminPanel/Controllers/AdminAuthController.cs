using Microsoft.AspNetCore.Mvc;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;
using BCrypt.Net;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AdminAuthController> _logger;

    public AdminAuthController(
        IAuthService authService, 
        IUserService userService, 
        ILogger<AdminAuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var userData = await _userService.GetActiveUserByUsername(request.Username);

            if (userData == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            if (userData.AccountLevel != "admin")
            {
                return Unauthorized(new { message = "Unauthorized access. Admin rights required." });
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, userData.Password))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = _authService.GenerateAdminToken(userData.Username);
            
            return Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin login");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
