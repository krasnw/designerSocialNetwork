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
}