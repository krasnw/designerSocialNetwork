using Back.Models;

namespace Back.Services.Interfaces
{
    public interface IChatService
    {
        // Request handling
        ChatRequestResult SendRequest(string username, Chat.Request request);
        Task<List<Chat.RequestResponse>> GetUserRequests(string username);
        Task<List<Chat.UserMiniProfile>> GetChatUsers(string username);
        Task<(RequestActionResult Result, string Message)> AcceptRequest(int requestId, string acceptingUsername);
        Task<bool> DeleteRequest(int requestId, string username);
        Task<bool> HasOpenRequest(string username1, string username2);

        // Message handling
        // Task<Chat.MessageText> SendTextMessage(string senderUsername, string receiverUsername, string content);
        Task<Chat.MessageComplex> SendComplexMessage(string senderUsername, Chat.MessageRequest request);
        Task<List<object>> GetConversation(string user1Username, string user2Username);

        // Transaction handling
        Task<Chat.MessageTransaction> SendTransactionMessage(string senderUsername, Chat.TransactionRequest request);
        Task<Chat.MessageTransactionApproval> ApproveTransaction(string transactionHash, string approverUsername);

        // Chat status
        Task<ChatStatusResult> GetChatStatus(string username1, string username2);

        // End request handling
        Task<Chat.MessageEndRequest> SendEndRequestMessage(string senderUsername, string receiverUsername);
        Task<Chat.MessageEndRequestApproval> ApproveEndRequest(string endRequestHash, string approverUsername);
    }
}
