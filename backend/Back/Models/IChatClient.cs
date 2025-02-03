using Back.Models;

namespace Back.Models
{
    public interface IChatClient
    {
        Task ReceiveMessage(Chat.MessageComplex message);
        Task ReceiveTransactionMessage(Chat.MessageTransaction message);
        Task ReceiveTransactionApproval(Chat.MessageTransactionApproval approval);
        Task ReceiveEndRequestMessage(Chat.MessageEndRequest request);
        Task ReceiveEndRequestApproval(Chat.MessageEndRequestApproval approval);
        Task ChatStatusChanged(object statusUpdate);
        Task MessageStatus(object status);
        Task TestResponse(string message);  // Add this method
    }
}
