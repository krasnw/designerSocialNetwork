using Back.Models;
using Back.Services;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ISubscriptionService _subscriptionService;

    public UserController(IUserService userService, ISubscriptionService subscriptionService)
    {
        _userService = userService;
        _subscriptionService = subscriptionService;
    }

    [Authorize]
    [HttpGet("profile/me")]
    public IActionResult GetMyProfile()
    {
        var uniqueName = User.Identity?.Name;
        var me = _userService.GetOwnProfile(uniqueName);
        return me != null ? Ok(me) : Unauthorized("Blame the token, relog please");
    }

    [HttpGet("profile/{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        if (string.IsNullOrEmpty(username)) return BadRequest("Username is required");

        var uniqueName = User.Identity?.Name;
        if (!string.IsNullOrEmpty(uniqueName) && uniqueName == username)
        {
            return RedirectToAction(nameof(GetMyProfile));
        }

        var user = _userService.GetProfile(username);
        var isSubscribed = await _subscriptionService.IsSubscribed(uniqueName, username);
        if (user != null)
        {
            return Ok(
                new
                {
                    user.Username,
                    user.FirstName,
                    user.LastName,
                    user.RatingPositions,
                    user.Description,
                    user.ProfileImage,
                    user.Rubies,
                    user.TotalLikes,
                    user.CompletedTasks,
                    IsSubscribedTo = isSubscribed
                }
            );
        } 
        else
        {
            return NotFound("User not found");
        }
    }
    
    
    //edit profile
    [Authorize]
    [HttpGet("profile/me/edit")]
    public IActionResult EditMyProfile()
    {
        var uniqueName = User.Identity?.Name;
        if (string.IsNullOrEmpty(uniqueName)) return Unauthorized("Blame the token, relog please");
        
        var user = _userService.EditData(uniqueName);  // Use instance method
        return user != null ? Ok(user) : Unauthorized("Blame the token, relog please");
    }
    
    [Authorize]
    [HttpPut("profile/me/edit")]
    public async Task<IActionResult> EditMyProfile([FromForm] User.EditRequest request)
    {
        var uniqueName = User.Identity?.Name;
        if (string.IsNullOrEmpty(uniqueName)) 
            return Unauthorized("Blame the token, relog please");
        
        try 
        {
            var success = await _userService.EditProfile(uniqueName, request);
            return success ? Ok() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred while updating the profile." });
        }
    }   
}