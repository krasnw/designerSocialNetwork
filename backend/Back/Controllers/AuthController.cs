using Back.Services;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Back.Models;

namespace Back.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> Login([FromBody] User.LoginRequest loginRequest)
    {
        try
        {
            var isValid = await _userService.Login(loginRequest.Username, loginRequest.Password);
            if (!isValid)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = await _authService.GenerateToken(loginRequest.Username);
            if (token == null)
            {
                return StatusCode(500, new { message = "Error generating token" });
            }

            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] User.SignUpRequest request)
    {
        try
        {
            var signUpResult = await _userService.SignUp(request.Username, request.Email, request.Password,
                request.FirstName, request.LastName, request.PhoneNumber, null);

            if (!string.IsNullOrEmpty(signUpResult))
            {
                return BadRequest(new { message = signUpResult });
            }

            var token = await _authService.GenerateToken(request.Username);
            return Ok(new { token });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("renew")]
    public async Task<IActionResult> RenewToken([FromBody] string token)
    {
        try
        {
            var newToken = await _authService.RenewToken(token);
            if (newToken == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            return Ok(new { token = newToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}