using System.Net.Http.Json;
using Core.Domain;
using Microsoft.JSInterop;

namespace Web.Services;

public class AuthService
{
    private readonly HttpClient m_httpClient;
    private readonly IJSRuntime m_jsRuntime;
    private readonly CustomAuthenticationStateProvider m_authStateProvider;
    private const string SESSION_TOKEN_KEY = "x-Session-Token";
    private const string USERNAME_KEY = "username";

    public AuthService(HttpClient p_httpClient, IJSRuntime p_jsRuntime, CustomAuthenticationStateProvider p_authStateProvider)
    {
        m_httpClient = p_httpClient;
        m_jsRuntime = p_jsRuntime;
        m_authStateProvider = p_authStateProvider;
    }

    public async Task<bool> LoginAsync(string p_username, string p_password)
    {
        LoginRequest loginRequest = new LoginRequest
        {
            Username = p_username,
            Password = p_password
        };

        HttpResponseMessage response = await m_httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            AuthResponse? authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse != null)
            {
                await m_jsRuntime.InvokeVoidAsync("localStorage.setItem", SESSION_TOKEN_KEY, authResponse.SessionToken);
                await m_jsRuntime.InvokeVoidAsync("localStorage.setItem", USERNAME_KEY, authResponse.Username);

                m_authStateProvider.NotifyUserAuthentication(authResponse.Username, authResponse.SessionToken);
                return true;
            }
        }

        return false;
    }

    public async Task LogoutAsync()
    {
        await m_jsRuntime.InvokeVoidAsync("localStorage.removeItem", SESSION_TOKEN_KEY);
        await m_jsRuntime.InvokeVoidAsync("localStorage.removeItem", USERNAME_KEY);
        m_authStateProvider.NotifyUserLogout();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await m_jsRuntime.InvokeAsync<string?>("localStorage.getItem", SESSION_TOKEN_KEY);
    }
}
