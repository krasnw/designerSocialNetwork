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

        public async Task SendMessage(Chat.MessageDto message)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            var result = await _chatService.SendMessage(senderUsername, message);
            await Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveMessage", result);
        }
        
        public async Task SendPaymentRequest(Chat.PaymentRequestDto request)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            var result = await _chatService.CreatePaymentRequest(request);
            await Clients.User(request.ReceiverUsername)
                .SendAsync("ReceivePaymentRequest", result);
        }

        public async Task SendTransactionMessage(Chat.TransactionMessage message)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            var result = await _chatService.SendTransactionMessage(senderUsername, message);
            await Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveTransactionMessage", result);
        }
    }
}