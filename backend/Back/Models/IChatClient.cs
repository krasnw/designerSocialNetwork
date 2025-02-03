using Back.Models;

namespace Back.Models
{
    public interface IChatClient
    {
        Task ReceiveMessage(object message);
        Task ReceiveTransactionMessage(Chat.MessageTransaction message);
        Task ReceiveTransactionApproval(Chat.MessageTransactionApproval message);
        Task ReceiveEndRequestMessage(Chat.MessageEndRequest message);
        Task ReceiveEndRequestApproval(Chat.MessageEndRequestApproval message);
        Task ChatStatusChanged(object statusUpdate);
        Task MessageStatus(object status);
    }
}
