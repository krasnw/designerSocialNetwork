namespace Back.Models;

public class Chat
{
    // Keep basic types
    public enum MessageType
    {
        Text = 0,
        Complex = 1,
        Transaction = 2,
        TransactionApproval = 3
    }

    // Keep request-related models
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

    // Message DTOs - all in one place
    public class MessageText
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverUsername { get; set; }
        public string TextContent { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
    }

    public class MessageComplex
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverUsername { get; set; }
        public string? TextContent { get; set; }
        public List<string> ImagePaths { get; set; } = new();
        public MessageType Type { get; set; } = MessageType.Complex;
    }

    public class MessageTransaction
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverUsername { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string TransactionNumber { get; set; }
        public string TransactionHash { get; set; }
        public bool IsApproved { get; set; }
        public MessageType Type { get; set; } = MessageType.Transaction;
        public DateTime CreatedAt { get; set; }
    }

    public class MessageTransactionApproval
    {
        public int Id { get; set; }
        public int OriginalTransactionMessageId { get; set; }
        public decimal Amount { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedAt { get; set; }
        public string TransactionNumber { get; set; }
        public string TransactionHash { get; set; }
        public MessageType Type { get; set; } = MessageType.TransactionApproval;
    }

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

    public class TransactionRequest
    {
        public string ReceiverUsername { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    public class PaymentRequest
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public int ReceiverId { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
    }

    public record PaymentRequestDto(
        int RequesterId,
        int ReceiverId,
        string RequesterUsername,
        string ReceiverUsername,
        decimal Amount
    );
}