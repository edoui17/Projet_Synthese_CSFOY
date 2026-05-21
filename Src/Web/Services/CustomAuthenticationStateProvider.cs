using System.Security.Claims;
using Core.Domain;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Web.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime m_jsRuntime;
    private const string SESSION_TOKEN_KEY = "x-Session-Token";
    private const string USERNAME_KEY = "username";

    public CustomAuthenticationStateProvider(IJSRuntime p_jsRuntime)
    {
        m_jsRuntime = p_jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string? token = await m_jsRuntime.InvokeAsync<string?>("localStorage.getItem", SESSION_TOKEN_KEY);
            string? username = await m_jsRuntime.InvokeAsync<string?>("localStorage.getItem", USERNAME_KEY);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            ClaimsIdentity identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("SessionToken", token)
            }, "IslandSurvivorAuth");

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyUserAuthentication(string p_username, string p_token)
    {
        ClaimsIdentity identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, p_username),
            new Claim("SessionToken", p_token)
        }, "IslandSurvivorAuth");

        ClaimsPrincipal user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }
}
