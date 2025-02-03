using Microsoft.AspNetCore.SignalR;
using Back.Models;
using Back.Services.Interfaces;

namespace Back.Models
{
    public class ChatHub : Hub<IChatClient>
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
            var username = Context.User?.Identity?.Name;
            _logger.LogInformation($"User attempting to connect: {username}");
            
            if (!string.IsNullOrEmpty(username))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, username);
                _logger.LogInformation($"User {username} connected with connection ID {Context.ConnectionId}");
            }
            else
            {
                _logger.LogWarning($"Connection attempted without username. ConnectionId: {Context.ConnectionId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);
                _logger.LogInformation($"User {username} disconnected");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(Chat.MessageRequest message)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            Console.WriteLine("Message received: {Message}", message);
            var result = await _chatService.SendComplexMessage(senderUsername, message);
            Console.WriteLine("Message sent: {Message}", result);
            await Clients.User(message.ReceiverUsername).ReceiveMessage(result);
            Console.WriteLine("Message received by receiver: {Message}", result);
        }

        public async Task SendTransactionMessage(Chat.TransactionRequest request)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            var result = await _chatService.SendTransactionMessage(senderUsername, request);
            await Clients.User(request.ReceiverUsername).ReceiveTransactionMessage(result);
        }

        public async Task SendTransactionApproval(Chat.MessageTransactionApproval approval)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            await Clients.User(approval.ReceiverUsername).ReceiveTransactionApproval(approval);
        }

        public async Task SendEndRequest(Chat.MessageEndRequest request)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            await Clients.User(request.ReceiverUsername).ReceiveEndRequestMessage(request);
        }

        public async Task SendEndRequestApproval(Chat.MessageEndRequestApproval approval, string[] usernames)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            await Clients.Users(usernames).ReceiveEndRequestApproval(approval);
        }

        public async Task NotifyChatStatusChanged(string[] usernames, object statusUpdate)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            await Clients.Users(usernames).ChatStatusChanged(statusUpdate);
        }

        public async Task SendMessageStatus(string username, object status)
        {
            await Clients.User(username).MessageStatus(status);
        }

        public async Task TestConnection()
        {
            try
            {
                await Clients.Caller.TestResponse("Test message from server");
                _logger.LogInformation("Test message sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in TestConnection: {ex.Message}");
                throw;
            }
        }
    }
}