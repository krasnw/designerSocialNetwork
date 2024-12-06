using Back.Models;

namespace Back.Services.Interfaces;

public interface IChatService
{
    bool SendRequest(string username, Chat.Request request);
}
