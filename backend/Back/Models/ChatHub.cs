using Microsoft.AspNetCore.SignalR;
using Back.Models;
using Back.Services.Interfaces;

namespace Back.Models
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, username);
            }
            await base.OnConnectedAsync();
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