using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate m_next;
    private const string API_KEY_HEADER_NAME = "X-API-KEY";

    public ApiKeyMiddleware(RequestDelegate p_next)
    {
        m_next = p_next;
    }

    public async Task InvokeAsync(HttpContext p_context)
    {
        if (!p_context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
        {
            p_context.Response.StatusCode = 401;
            await p_context.Response.WriteAsync("API Key was not provided.");
            return;
        }

        var configuration = p_context.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = configuration.GetValue<string>("ApiKey");

        if (apiKey == null || extractedApiKey != apiKey)
        {
            p_context.Response.StatusCode = 401;
            await p_context.Response.WriteAsync("Unauthorized client.");
            return;
        }

        await m_next(p_context);
    }
}
