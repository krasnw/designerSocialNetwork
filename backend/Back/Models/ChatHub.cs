using Microsoft.AspNetCore.SignalR;
using Back.Models;

namespace Back.Models
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(Chat.MessageDto message)
        {
            await Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveMessage", message);
        }
        
        public async Task SendPaymentRequest(Chat.PaymentRequestDto request)
        {
            await Clients.User(request.ReceiverUsername)
                .SendAsync("ReceivePaymentRequest", request);
        }
    }
}