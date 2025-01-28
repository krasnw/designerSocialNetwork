using Back.Models;

namespace Back.Services.Interfaces
{
    public interface IChatService
    {
        ChatRequestResult SendRequest(string username, Chat.Request request);
        Task<List<Chat.RequestResponse>> GetUserRequests(string username);
        Task<List<string>> GetChatUsers(string username);
        Task<bool> AcceptRequest(int requestId);
        Task<bool> DeleteRequest(int requestId);
        Task<Chat.Message> SendMessage(string senderUsername, Chat.MessageDto message);
        Task<List<Chat.Message>> GetConversation(string user1Username, string user2Username);
        Task<Chat.PaymentRequest> CreatePaymentRequest(Chat.PaymentRequestDto request);
        Task<Chat.TransactionMessageResponse> SendTransactionMessage(string senderUsername, Chat.TransactionMessage message);
        Task<bool> ApproveTransaction(int messageId, string approverUsername);
    }
}
