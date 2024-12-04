using Back.Models;
using Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("profile/me")]
    public IActionResult GetMyProfile()
    {
        var uniqueName = User.Identity?.Name;

        var me = UserService.GetOwnProfile(uniqueName);
        return me != null ? Ok(me) : Unauthorized("Blame the token, relog please");
    }

    [HttpGet("profile/{username}")]
    public IActionResult GetUser(string username)
    {
        if (string.IsNullOrEmpty(username)) return BadRequest("Username is required");

        var uniqueName = User.Identity?.Name;
        if (!string.IsNullOrEmpty(uniqueName) && uniqueName == username)
        {
            return RedirectToAction(nameof(GetMyProfile));
        }

        var user = UserService.GetProfile(username);
        return user != null ? Ok(user) : NotFound();
    }
    
    
    //edit profile
    [Authorize]
    [HttpGet("profile/me/edit")]
    public IActionResult EditMyProfile()
    {
        var uniqueName = User.Identity?.Name;
        if (string.IsNullOrEmpty(uniqueName)) return Unauthorized("Blame the token, relog please");
        
        var user = UserService.EditData(uniqueName);
        return user != null ? Ok(user) : Unauthorized("Blame the token, relog please");
    }
    
    [Authorize]
    [HttpPut("profile/me/edit")]
    public IActionResult EditMyProfile([FromBody] User.EditRequest request)
    {
        var uniqueName = User.Identity?.Name;
        if (string.IsNullOrEmpty(uniqueName)) return Unauthorized("Blame the token, relog please");
        
        var user = UserService.EditProfile(uniqueName, request);
        return user != null ? Ok(user) : NotFound();
    }   
}