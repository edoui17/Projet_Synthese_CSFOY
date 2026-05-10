using System.Linq;
using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace API.Middleware;

public class SessionAuthMiddleware
{
    private readonly RequestDelegate m_next;
    private const string SESSION_TOKEN_HEADER_NAME = "X-Session-Token";

    public SessionAuthMiddleware(RequestDelegate p_next)
    {
        m_next = p_next;
    }

    public async Task InvokeAsync(HttpContext p_context)
    {
        // Skip authentication for specific endpoints
        var path = p_context.Request.Path.Value?.ToLower();
        if (path != null && (path.Contains("/auth/login") || path.Contains("/player/leaderboard")))
        {
            await m_next(p_context);
            return;
        }

        if (!p_context.Request.Headers.TryGetValue(SESSION_TOKEN_HEADER_NAME, out var extractedToken))
        {
            // Allow processing but specific controllers will handle 401 if they need the player
            await m_next(p_context);
            return;
        }

        var authRepository = p_context.RequestServices.GetRequiredService<IAuthRepository>();
        var player = await authRepository.GetBySessionTokenAsync(extractedToken.ToString());

        if (player != null)
        {
            p_context.Items["Player"] = player;
        }

        await m_next(p_context);
    }
}
