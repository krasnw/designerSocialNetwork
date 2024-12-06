using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Back.Services;

public interface IAuthService
{
    void AddAuth(IServiceCollection services);
    string? GenerateToken(string username);
    string? RenewToken(string token);
    ClaimsPrincipal? ValidateToken(string token);
}
