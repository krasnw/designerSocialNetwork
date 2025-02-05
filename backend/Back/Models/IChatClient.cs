using Back.Models;

namespace Back.Models
{
    public interface IChatClient
    {
        Task ReceiveMessage(Chat.MessageComplex message, string messageId);
        Task ReceiveTransactionMessage(Chat.MessageTransaction message, string messageId);
        Task ReceiveTransactionApproval(Chat.MessageTransactionApproval approval, string messageId);
        Task ReceiveEndRequestMessage(Chat.MessageEndRequest request, string messageId);
        Task ReceiveEndRequestApproval(Chat.MessageEndRequestApproval approval, string messageId);
        Task ChatStatusChanged(object statusUpdate);
        Task MessageStatus(object status);
        Task TestResponse(string message);
        Task Heartbeat();
    }
}
