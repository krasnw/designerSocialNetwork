using Back.Models;

namespace Back.Services.Interfaces;

public interface IUserService
{
    string SignUp(string username, string email, string password, string firstName,
        string lastName, string phoneNumber, string profileImage);
    Task<bool> Login(string username, string password);
    bool Logout(string username);
    bool IsLoggedIn(string username);
    User? GetUser(int id);
    User? GetUser(string username);
    UserProfile? GetOwnProfile(string username);
    UserProfile? GetProfile(string username);
    User.EditDataResponse? EditData(string username);
    Task<bool> EditBasicProfile(string username, User.EditBasicRequest request);
    Task<bool> EditSensitiveProfile(string username, User.EditSensitiveRequest request);
}
