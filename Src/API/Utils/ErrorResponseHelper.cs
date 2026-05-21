using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace API.Utils;

public static class ErrorResponseHelper
{
    public static Task WriteErrorResponseAsync(HttpContext p_context, HttpStatusCode p_statusCode, string p_error, string p_message)
    {
        p_context.Response.ContentType = "application/json";
        p_context.Response.StatusCode = (int)p_statusCode;

        var errorResponse = new
        {
            error = p_error,
            message = p_message,
            timestamp = DateTime.UtcNow.ToString("O")
        };

        var result = JsonSerializer.Serialize(errorResponse);
        return p_context.Response.WriteAsync(result);
    }
}
