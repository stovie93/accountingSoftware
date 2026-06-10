using System.Security.Claims;
using ConserviceAccounting.Api.Services;

namespace ConserviceAccounting.Api.Middleware;

public class UserMiddleware
{
    private readonly RequestDelegate _next;

    public UserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserContext userContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                userContext.UserId = userId;
            }

            userContext.UserRole = roleClaim ?? string.Empty;
        }

        await _next(context);
    }
}
