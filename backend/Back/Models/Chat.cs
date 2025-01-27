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
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public enum MessageType
    {
        Text,
        Image,
        PaymentRequest
    }

    // Message model
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public record MessageDto(
        int SenderId,
        int ReceiverId,
        string SenderUsername,    // Added
        string ReceiverUsername,  // Added
        string Content,
        MessageType Type
    );

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
}