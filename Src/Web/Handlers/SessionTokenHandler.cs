using System.Net;
using Microsoft.JSInterop;

namespace Web.Handlers;

public class SessionTokenHandler : DelegatingHandler
{
    private readonly IJSRuntime m_jsRuntime;
    private const string SESSION_TOKEN_KEY = "x-Session-Token";
    private const string USERNAME_KEY = "username";

    public SessionTokenHandler(IJSRuntime p_jsRuntime)
    {
        m_jsRuntime = p_jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage p_request, CancellationToken p_cancellationToken)
    {
        string? token = null;

        try
        {
            token = await m_jsRuntime.InvokeAsync<string?>("localStorage.getItem", SESSION_TOKEN_KEY);
        }
        catch (InvalidOperationException)
        {
            // JS Interop might not be available yet during prerendering
        }

        if (!string.IsNullOrEmpty(token))
        {
            p_request.Headers.Add("x-Session-Token", token);
        }

        HttpResponseMessage response = await base.SendAsync(p_request, p_cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            try
            {
                await m_jsRuntime.InvokeVoidAsync("localStorage.removeItem", SESSION_TOKEN_KEY);
                await m_jsRuntime.InvokeVoidAsync("localStorage.removeItem", USERNAME_KEY);
                // Note: We don't notify Logout here because we are in a low-level handler.
                // The next time the UI checks the state via CustomAuthenticationStateProvider, it will see no token.
            }
            catch (InvalidOperationException) { }
        }

        return response;
    }
}
