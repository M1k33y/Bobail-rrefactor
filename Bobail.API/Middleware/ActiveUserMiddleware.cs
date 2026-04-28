using System.Security.Claims;
using System.Text.Json;
using Bobail.Application.Interfaces.Repositories;

namespace Bobail.API.Middleware;

public class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdValue = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                await WriteResponseAsync(context, StatusCodes.Status401Unauthorized, "Invalid user token.");
                return;
            }

            var user = await userRepository.GetByIdAsync(userId);

            if (user is null || !user.IsActive)
            {
                await WriteResponseAsync(context, StatusCodes.Status403Forbidden, "This user is currently banned.");
                return;
            }
        }

        await _next(context);
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = message
        }));
    }
}
