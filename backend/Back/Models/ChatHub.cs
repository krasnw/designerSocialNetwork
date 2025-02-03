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
                if (string.IsNullOrEmpty(username))
                {
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

            var result = await _chatService.SendComplexMessage(senderUsername, message);
            await Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveMessage", result);
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
    }
}