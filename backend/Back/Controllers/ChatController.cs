using Back.Models;
using Back.Services;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [Authorize]
    [HttpPost("sendRequest")]
    public IActionResult SendRequest([FromBody] Chat.Request request)
    {
        var uniqueName = User.Identity?.Name;
        if (string.IsNullOrEmpty(uniqueName)) return Unauthorized("Blame the token, relog please");
        
        var success = _chatService.SendRequest(uniqueName, request);
        return success ? Ok() : BadRequest();
    }

    [Authorize]
    [HttpGet("requests")]
    public async Task<ActionResult<List<Chat.RequestResponse>>> GetRequests()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return Unauthorized("Blame the token, relog please");

        try
        {
            var requests = await _chatService.GetUserRequests(username);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving requests: {ex.Message}");
        }
    }
}