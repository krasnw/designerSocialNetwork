using Microsoft.AspNetCore.SignalR;
using Back.Models;
using Back.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Back.Models
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var username = Context.User?.Identity?.Name;
                _logger.LogInformation($"Connection attempt - User identity: {Context.User?.Identity?.IsAuthenticated}, Username: {username}");
                
                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Unauthorized connection attempt - no username");
                    throw new HubException("Unauthorized connection attempt");
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, username);
                _logger.LogInformation($"User {username} connected with connection ID {Context.ConnectionId}");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OnConnectedAsync: {ex.Message}");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var username = Context.User?.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);
                    _logger.LogInformation($"User {username} disconnected");
                }
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OnDisconnectedAsync: {ex.Message}");
                throw;
            }
        }

        public async Task SendMessage(Chat.MessageRequest message)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            _logger.LogInformation($"Sending message from {senderUsername} to {message.ReceiverUsername}");
            var result = await _chatService.SendComplexMessage(senderUsername, message);
            _logger.LogInformation($"Message compiled: {result}");
            await Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveMessage", result);
            _logger.LogInformation($"Message sent to {message.ReceiverUsername}");
        }

        public async Task SendTransactionMessage(Chat.TransactionRequest request)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            var result = await _chatService.SendTransactionMessage(senderUsername, request);
            await Clients.User(request.ReceiverUsername)
                .SendAsync("ReceiveTransactionMessage", result);
        }

        public async Task TestConnection()
        {
            try
            {
                var username = Context.User?.Identity?.Name ?? "Anonymous";
                await Clients.Caller.SendAsync("TestResponse", $"Connection successful for {username}");
                _logger.LogInformation($"Test connection successful for user {username}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Test connection failed: {ex.Message}");
                throw;
            }
        }
    }
}