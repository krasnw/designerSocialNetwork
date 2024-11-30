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
        var user = UserService.GetUser(User.Identity?.Name);
        return user != null ? Ok(user) : NotFound();
    }
    
    // [HttpGet("profile/{id}")]
    // public IActionResult GetUser(int id)
    // {
    //     var user = _userService.GetUser(id);
    //     return user != null ? Ok(user) : NotFound();
    // }
    
    // [HttpPost("users")]
    // public IActionResult CreateUser([FromBody] User user)
    // {
    //     if (_userService.CreateUser(user))
    //     {
    //         return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    //     }
    //
    //     return BadRequest();
    // }
    //
    // [HttpPut("users/{id}")]
    // public IActionResult UpdateUser(int id, [FromBody] User user)
    // {
    //     if (_userService.UpdateUser(id, user))
    //     {
    //         return Ok();
    //     }
    //
    //     return NotFound();
    // }
    //
    // [HttpDelete("users/{id}")]
    // public IActionResult DeleteUser(int id)
    // {
    //     if (_userService.DeleteUser(id))
    //     {
    //         return Ok();
    //     }
    //
    //     return NotFound();
    // }
}