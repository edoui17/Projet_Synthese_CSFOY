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
        // Whitelist login, leaderboard and swagger
        if (path != null && (path.StartsWith("/api/auth/login") || path.StartsWith("/api/player/leaderboard") || path.StartsWith("/swagger")))
        {
            await m_next(p_context);
            return;
        }

        if (!p_context.Request.Headers.TryGetValue(SESSION_TOKEN_HEADER_NAME, out var extractedToken))
        {
            await API.Utils.ErrorResponseHelper.WriteErrorResponseAsync(p_context, System.Net.HttpStatusCode.Unauthorized, "Unauthorized", "Session token is missing.");
            return;
        }

        var authRepository = p_context.RequestServices.GetRequiredService<IAuthRepository>();
        var player = await authRepository.GetBySessionTokenAsync(extractedToken.ToString());

        if (player == null)
        {
            await API.Utils.ErrorResponseHelper.WriteErrorResponseAsync(p_context, System.Net.HttpStatusCode.Unauthorized, "Unauthorized", "Invalid or expired session token.");
            return;
        }

        p_context.Items["Player"] = player;
        await m_next(p_context);
    }
}
