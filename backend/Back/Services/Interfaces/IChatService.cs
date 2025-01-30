using Back.Models;

namespace Back.Services.Interfaces
{
    public interface IChatService
    {
        ChatRequestResult SendRequest(string username, Chat.Request request);
        Task<List<Chat.RequestResponse>> GetUserRequests(string username);
        Task<List<Chat.UserMiniProfile>> GetChatUsers(string username);  // Changed return type
        Task<(RequestActionResult Result, string Message)> AcceptRequest(int requestId, string acceptingUsername);
        Task<bool> DeleteRequest(int requestId, string username);
        Task<Chat.Message> SendMessage(string senderUsername, Chat.MessageDto message);
        Task<List<Chat.Message>> GetConversation(string user1Username, string user2Username);
        Task<Chat.PaymentRequest> CreatePaymentRequest(Chat.PaymentRequestDto request);
        Task<Chat.TransactionMessageResponse> SendTransactionMessage(string senderUsername, Chat.TransactionMessage message);
        Task<bool> ApproveTransaction(string transactionHash, string approverUsername);
        Task<bool> HasOpenRequest(string username1, string username2);
    }
}
