using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Back.Services.Interfaces;

public interface IAuthService
{
    void AddAuth(IServiceCollection services);
    Task<string?> GenerateToken(string username);
    Task<string?> RenewToken(string token);
    ClaimsPrincipal? ValidateToken(string token);
}
