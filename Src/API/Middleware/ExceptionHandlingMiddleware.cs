using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate m_next;

    public ExceptionHandlingMiddleware(RequestDelegate p_next)
    {
        m_next = p_next;
    }

    public async Task InvokeAsync(HttpContext p_context)
    {
        try
        {
            await m_next(p_context);
        }
        catch (SqlException p_ex)
        {
            await HandleDatabaseExceptionAsync(p_context, p_ex);
        }
        catch (InvalidOperationException p_ex) when (p_ex.Message.Contains("database", StringComparison.OrdinalIgnoreCase))
        {
             await HandleDatabaseExceptionAsync(p_context, p_ex);
        }
    }

    private static Task HandleDatabaseExceptionAsync(HttpContext p_context, Exception p_exception)
    {
        p_context.Response.ContentType = "application/json";
        p_context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

        var result = JsonSerializer.Serialize(new { error = "Database is temporarily offline" });
        return p_context.Response.WriteAsync(result);
    }
}
