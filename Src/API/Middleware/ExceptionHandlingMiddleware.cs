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

        var statusCode = HttpStatusCode.InternalServerError;
        var error = "Internal Server Error";
        var message = "An unexpected error occurred while processing your request.";

        // US 20.0.2: Map infrastructure/database failures to 503 Service Unavailable
        if (p_exception is SqlException ||
            p_exception is Microsoft.EntityFrameworkCore.DbUpdateException ||
            p_exception.InnerException is SqlException)
        {
            statusCode = HttpStatusCode.ServiceUnavailable;
            error = "Service Unavailable";
            message = "The game server is currently unable to reach the database. Please try again later.";
        }

        return API.Utils.ErrorResponseHelper.WriteErrorResponseAsync(p_context, statusCode, error, message);
    }
}
