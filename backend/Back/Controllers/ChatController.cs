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
            return Unauthorized(new { message = "Blame the token, relog please" });
        
        if (uniqueName.Equals(request.Receiver, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Cannot send chat request to yourself" });
    
        try
        {
            var result = _chatService.SendRequest(uniqueName, request);
            return result switch
            {
                ChatRequestResult.Success => Ok(new { message = "Request sent successfully" }),
                ChatRequestResult.ReceiverNotFound => NotFound(new { message = "Receiver not found" }),
                ChatRequestResult.SenderNotFound => Unauthorized(new { message = "Sender not found" }),
                _ => BadRequest(new { message = "Request failed" })
            };
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return BadRequest(new { message = "An error occurred while sending the request" });
        }
    }

    [Authorize]
    [HttpGet("requests")]
    public async Task<ActionResult<List<Chat.RequestResponse>>> GetRequests()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) 
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var requests = await _chatService.GetUserRequests(username);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error retrieving requests: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpGet("users")]
    public async Task<ActionResult<List<Chat.UserMiniProfile>>> GetChatUsers()  // Changed return type
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) 
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var users = await _chatService.GetChatUsers(username);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error retrieving chat users: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPost("requests/{requestId}/accept")]
    public async Task<IActionResult> AcceptRequest(int requestId)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var (result, message) = await _chatService.AcceptRequest(requestId, username);
            
            return result switch
            {
                RequestActionResult.Success => Ok(new { message }),
                RequestActionResult.NotFound => NotFound(new { message }), // Will now properly return 404
                RequestActionResult.AlreadyAccepted => BadRequest(new { message }),
                RequestActionResult.NotSeller => BadRequest(new { message }),
                RequestActionResult.Error => BadRequest(new { message }),
                _ => StatusCode(500, new { message = "An unexpected error occurred" })
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Server error: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpDelete("requests/{requestId}")]
    public async Task<IActionResult> DeleteRequest(int requestId)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var success = await _chatService.DeleteRequest(requestId, username);
            if (!success)
            {
                return BadRequest(new { message = "Request cannot be deleted. Either it doesn't exist or you're not the seller." });
            }
            return Ok(new { message = "Request deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error deleting request: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPost("messages")]
    public async Task<ActionResult<Chat.MessageComplex>> SendMessage([FromForm] Chat.MessageRequest request)
    {
        var senderUsername = User.Identity?.Name;
        if (string.IsNullOrEmpty(senderUsername))
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var result = await _chatService.SendComplexMessage(senderUsername, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error sending message: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPost("transaction")]
    public async Task<ActionResult<Chat.MessageTransaction>> SendTransactionMessage(
        [FromBody] Chat.TransactionRequest request)
    {
        var senderUsername = User.Identity?.Name;
        if (string.IsNullOrEmpty(senderUsername))
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var result = await _chatService.SendTransactionMessage(senderUsername, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error sending transaction: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPost("transaction/{transactionHash}/approve")]
    public async Task<ActionResult<Chat.MessageTransactionApproval>> ApproveTransaction(string transactionHash)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var result = await _chatService.ApproveTransaction(transactionHash, username);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error approving transaction: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpGet("conversations/{otherUsername}")]
    public async Task<ActionResult<List<object>>> GetConversation(string otherUsername)
    {
        var currentUsername = User.Identity?.Name;
        if (string.IsNullOrEmpty(currentUsername))
            return Unauthorized(new { message = "Blame the token, relog please" });

        try 
        {
            var messages = await _chatService.GetConversation(currentUsername, otherUsername);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error retrieving conversation: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpGet("hasOpenRequest/{otherUsername}")]
    public async Task<ActionResult<bool>> HasOpenRequest(string otherUsername)
    {
        var currentUsername = User.Identity?.Name;
        if (string.IsNullOrEmpty(currentUsername))
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var hasOpen = await _chatService.HasOpenRequest(currentUsername, otherUsername);
            return Ok(new { hasOpenRequest = hasOpen });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error checking request status: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpGet("chatStatus/{otherUsername}")]
    public async Task<ActionResult<string>> GetChatStatus(string otherUsername)
    {
        var currentUsername = User.Identity?.Name;
        if (string.IsNullOrEmpty(currentUsername))
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var status = await _chatService.GetChatStatus(currentUsername, otherUsername);
            var statusString = status switch
            {
                ChatStatusResult.Active => "active",
                ChatStatusResult.Disabled => "disabled",
                ChatStatusResult.NonExistent => "does not exist",
                _ => "unknown"
            };
            return Ok(new { chatStatus = statusString });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error retrieving chat status: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPost("endRequest/{otherUsername}")]
    public async Task<ActionResult<Chat.MessageEndRequest>> SendEndRequestMessage(string otherUsername)
    {
        var senderUsername = User.Identity?.Name;
        if (string.IsNullOrEmpty(senderUsername))
            return Unauthorized(new { message = "Blame the token, relog please" });

        if (string.IsNullOrEmpty(otherUsername))
            return BadRequest(new { message = "Receiver username is required" });

        try
        {
            var result = await _chatService.SendEndRequestMessage(senderUsername, otherUsername);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error sending end request: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPost("endRequest/{endRequestHash}/approve")]
    public async Task<ActionResult<Chat.MessageEndRequestApproval>> ApproveEndRequest(string endRequestHash)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized(new { message = "Blame the token, relog please" });

        try
        {
            var result = await _chatService.ApproveEndRequest(endRequestHash, username);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error approving end request: {ex.Message}" });
        }
    }
}