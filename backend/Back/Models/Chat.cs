namespace Back.Models;

public class Chat
{
    public class Request
    {
        public string Receiver { get; set; }
        public string Description { get; set; }
    }

    public class RequestResponse
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public UserMiniProfile SenderProfile { get; set; }
    }

    public class UserMiniProfile
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfileImage { get; set; }
    }

    public enum MessageType
    {
        Text = 0,
        Complex = 1,
        Transaction = 2,
        TransactionApproval = 3
    }

    // Message model
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverUsername { get; set; }
        public string? TextContent { get; set; }
        public List<string> ImagePaths { get; set; } = new();
        public MessageType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int? ApprovedByMessageId { get; set; }
        public string? TransactionNumber { get; set; }
        public string? TransactionHash { get; set; }  // Add this line
        public int? ChatId { get; set; }
    }

    // New class for form data
    public class MessageRequest
    {
        public string ReceiverUsername { get; set; }
        public string? TextContent { get; set; }
        public IFormFile[]? Images { get; set; }
    }

    public record MessageDto
    {
        public string ReceiverUsername { get; init; }
        public string? TextContent { get; init; }
        public List<IFormFile>? Images { get; init; }
        public MessageType Type { get; init; }
    }

    // PaymentRequest model
    public class PaymentRequest
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public int ReceiverId { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public int MessageId { get; set; }
    }

    public record PaymentRequestDto(
        int RequesterId,
        int ReceiverId,
        string RequesterUsername,    // Added
        string ReceiverUsername,     // Added
        decimal Amount
    );

    // Add new Transaction-specific models
    public class TransactionMessage
    {
        public string ReceiverUsername { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    public class TransactionMessageResponse
    {
        public int MessageId { get; set; }
        public string TransactionNumber { get; set; }
        public string TransactionHash { get; set; }  // Added this line
        public int ChatId { get; set; }
        public string SenderUsername { get; set; }
        public string ReceiverUsername { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageType Type { get; set; } = MessageType.Transaction;
        public int? ApprovalMessageId { get; set; }
    }

    public class TransactionApprovalMessage
    {
        public int Id { get; set; }
        public int OriginalTransactionMessageId { get; set; }
        public string TransactionHash { get; set; }  // Added this line
        public decimal Amount { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedAt { get; set; }
        public MessageType Type { get; set; } = MessageType.TransactionApproval;
        public string TransactionNumber { get; set; }  // Add this
        public int ChatId { get; set; }  // Add this
    }
}