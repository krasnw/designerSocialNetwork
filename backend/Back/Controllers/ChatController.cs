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
        if (string.IsNullOrEmpty(uniqueName)) 
            return Unauthorized("Blame the token, relog please");
        
        if (uniqueName.Equals(request.Receiver, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Cannot send chat request to yourself");
    
        try
        {
            var result = _chatService.SendRequest(uniqueName, request);
            return result switch
            {
                ChatRequestResult.Success => Ok(),
                ChatRequestResult.ReceiverNotFound => NotFound("Receiver not found"),
                ChatRequestResult.SenderNotFound => Unauthorized("Sender not found"),
                _ => BadRequest("Request failed")
            };
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
    public async Task<ActionResult<Chat.Message>> SendMessage([FromForm] Chat.MessageRequest request)
    {
        var senderUsername = User.Identity?.Name;
        if (string.IsNullOrEmpty(senderUsername))
            return Unauthorized("Blame the token, relog please");

        if (senderUsername == request.ReceiverUsername)
            return BadRequest("Cannot send message to yourself");

        try
        {
            // Convert request to MessageDto
            var messageDto = new Chat.MessageDto
            {
                ReceiverUsername = request.ReceiverUsername,
                TextContent = request.TextContent,
                Images = request.Images?.ToList(),
                Type = Chat.MessageType.Complex // Will be determined by the service
            };

            var result = await _chatService.SendMessage(senderUsername, messageDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error sending message: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost("transaction")]
    public async Task<ActionResult<Chat.TransactionMessageResponse>> SendTransactionMessage(
        [FromBody] Chat.TransactionMessage request)
    {
        var senderUsername = User.Identity?.Name;
        if (string.IsNullOrEmpty(senderUsername))
            return Unauthorized("Blame the token, relog please");

        if (senderUsername == request.ReceiverUsername)
            return BadRequest("Cannot send transaction to yourself");

        try
        {
            var result = await _chatService.SendTransactionMessage(senderUsername, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error sending transaction message: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost("transaction/{messageId}/approve")]
    public async Task<IActionResult> ApproveTransaction(int messageId)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized("Blame the token, relog please");

        try
        {
            var result = await _chatService.ApproveTransaction(messageId, username);
            return result ? Ok("Transaction approved and processed") : BadRequest("Could not process transaction");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error approving transaction: {ex.Message}");
        }
    }

    [Authorize]
    [HttpGet("conversations/{otherUsername}")]
    public async Task<ActionResult<List<Chat.Message>>> GetConversation(string otherUsername)
    {
        var currentUsername = User.Identity?.Name; // From JWT token
        var messages = await _chatService.GetConversation(currentUsername, otherUsername);
        return Ok(messages);
    }
}