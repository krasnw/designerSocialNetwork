using Back.Services.Interfaces;
using System.Security.Claims;

namespace Back.Middleware;

public class FrozenUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDatabaseService _databaseService;

    public FrozenUserMiddleware(RequestDelegate next, IDatabaseService databaseService)
    {
        _next = next;
        _databaseService = databaseService;
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

        var username = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(username))
        {
            var userStatus = await _databaseService.GetUserStatus(username);
            if (userStatus == "frozen")
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Account is frozen" });
                return;
            }
        }

        await _next(context);
    }
}
