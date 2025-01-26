using Back.Models;

namespace Back.Services.Interfaces;

public interface IChatService
{
    bool SendRequest(string username, Chat.Request request);
    Task<List<Chat.RequestResponse>> GetUserRequests(string username);
}
