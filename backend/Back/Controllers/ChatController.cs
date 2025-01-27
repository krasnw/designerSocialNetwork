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
        
        try
        {
            var success = _chatService.SendRequest(uniqueName, request);
            return success ? Ok() : BadRequest();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return BadRequest("An error occurred while sending the request");
        }
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

    [Authorize]
    [HttpGet("users")]
    public async Task<ActionResult<List<string>>> GetChatUsers()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return Unauthorized("Blame the token, relog please");

        try
        {
            var users = await _chatService.GetChatUsers(username);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving chat users: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost("requests/{requestId}/accept")]
    public async Task<IActionResult> AcceptRequest(int requestId)
    {
        try
        {
            var success = await _chatService.AcceptRequest(requestId);
            return success 
                ? Ok() 
                : NotFound("Request not found or is not in pending state");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error accepting request: {ex.Message}");
        }
    }

    [Authorize]
    [HttpDelete("requests/{requestId}")]
    public async Task<IActionResult> DeleteRequest(int requestId)
    {
        try
        {
            var success = await _chatService.DeleteRequest(requestId);
            return success ? Ok() : BadRequest("Could not delete request");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error deleting request: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost("messages")]
    public async Task<ActionResult<Message>> SendMessage(MessageDto message)
    {
        var result = await _chatService.SendMessage(message);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("conversations/{otherUserId}")]
    public async Task<ActionResult<List<Message>>> GetConversation(int otherUserId)
    {
        var currentUserId = GetCurrentUserId(); // From JWT token
        var messages = await _chatService.GetConversation(currentUserId, otherUserId);
        return Ok(messages);
    }

    private int GetCurrentUserId()
    {
        // Implement logic to get current user ID from JWT token
        return int.Parse(User.Claims.First(c => c.Type == "id").Value);
    }
}