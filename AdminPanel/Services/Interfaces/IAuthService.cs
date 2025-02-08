using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace AdminPanel.Services.Interfaces;

public interface IAuthService
{
    void AddAuth(IServiceCollection services);
    string? GenerateToken(string username);
    string? GenerateAdminToken(string username);
    string? RenewToken(string token);
    ClaimsPrincipal? ValidateToken(string token);
}
