// ChatHub.cs
public class ChatHub : Hub
{
    public async Task SendMessage(MessageDto message)
    {
        await Clients.User(message.ReceiverId.ToString())
            .SendAsync("ReceiveMessage", message);
    }
    
    public async Task SendPaymentRequest(PaymentRequestDto request)
    {
        await Clients.User(request.ReceiverId.ToString())
            .SendAsync("ReceivePaymentRequest", request);
    }
}