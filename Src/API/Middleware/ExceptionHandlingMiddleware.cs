using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate m_next;
    private readonly ILogger<ExceptionHandlingMiddleware> m_logger;

    public ExceptionHandlingMiddleware(RequestDelegate p_next, ILogger<ExceptionHandlingMiddleware> p_logger)
    {
        m_next = p_next;
        m_logger = p_logger;
    }

    public async Task InvokeAsync(HttpContext p_context)
    {
        try
        {
            await m_next(p_context);
        }
        // SECURITY FIX: Catch all exceptions to prevent unhandled exceptions from leaking native framework
        // stack traces and infrastructure details directly to the player HUD.
        catch (Exception p_ex)
        {
            await HandleExceptionAsync(p_context, p_ex, m_logger);
        }
    }

    private static Task HandleExceptionAsync(HttpContext p_context, Exception p_exception, ILogger p_logger)
    {
        // SECURITY FIX: Secure error masking (Logs details locally, hides system internals from player UI)
        p_logger.LogError(p_exception, "An unhandled exception occurred during the request pipeline.");

        p_context.Response.ContentType = "application/json";
        p_context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var errorResponse = new
        {
            error = "Internal Server Error",
            message = "An unexpected error occurred while processing your request.",
            timestamp = DateTime.UtcNow.ToString("O")
        };

        var result = JsonSerializer.Serialize(errorResponse);
        return p_context.Response.WriteAsync(result);
    }
}
