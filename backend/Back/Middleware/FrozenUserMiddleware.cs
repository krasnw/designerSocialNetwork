using Back.Services.Interfaces;
using System.Security.Claims;

namespace Back.Middleware;

public class FrozenUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public FrozenUserMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip checking for health endpoint and authentication endpoints
        if (context.Request.Path.StartsWithSegments("/health") || 
            context.Request.Path.StartsWithSegments("/api/auth"))
        {
            await _next(context);
            return;
        }
        if (context.User.Identity?.Name != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
            
            var username = context.User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = await dbService.GetUserStatus(username);
                if (user == "frozen")
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "Account is frozen" });
                    return;
                }
            }
        }
        await _next(context);
    }
}