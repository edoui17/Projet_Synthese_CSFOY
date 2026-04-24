using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;

namespace Core.Services;

public class ApiService : IApiService
{
    private readonly HttpClient m_httpClient;
    private readonly ISaveService m_saveService;
    private string? m_sessionToken;
    private const string CACHE_FILE = "profile_cache.json";

    // We are going to use default options to handle circular ref just in case
    private readonly JsonSerializerOptions m_jsonOptions;

    public ApiService(ISaveService p_saveService)
    {
        m_httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5271")
        };
        m_saveService = p_saveService;
        m_jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
    }

    public async Task<string?> LoginAsync(string p_username, string p_password)
    {
        try
        {
            LoginRequest request = new LoginRequest { Username = p_username, Password = p_password };
            HttpResponseMessage response = await m_httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                LoginResponse? result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                m_sessionToken = result?.SessionToken;
                return m_sessionToken;
            }
        }
        catch (HttpRequestException)
        {
            // API is down, swallow error to allow offline mode
        }
        return null;
    }

    public async Task<PlayerProfile?> GetProfileAsync()
    {
        if (!string.IsNullOrEmpty(m_sessionToken))
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/api/player/profile");
                request.Headers.Add("X-Session-Token", m_sessionToken);

                HttpResponseMessage response = await m_httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    PlayerProfile? profile = await response.Content.ReadFromJsonAsync<PlayerProfile>(m_jsonOptions);
                    if (profile != null)
                    {
                        CacheProfileLocally(profile);
                        return profile;
                    }
                }
            }
            catch (HttpRequestException)
            {
                // API is down, try local cache
            }
        }

        // Offline fallback
        return GetCachedProfile();
    }

    public async Task<bool> SyncAsync(SyncRequest p_request)
    {
        // Add session token if we have one, otherwise it's just local save
        p_request.SessionToken = m_sessionToken ?? string.Empty;

        // Update local cache to reflect current state
        UpdateLocalCache(p_request);

        if (string.IsNullOrEmpty(p_request.SessionToken))
        {
            // We are offline and don't have a token, we just rely on local cache updated above
            return false;
        }

        try
        {
            HttpResponseMessage response = await m_httpClient.PostAsJsonAsync("/api/player/sync", p_request, m_jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            // API down, offline fallback only
            return false;
        }
    }

    private void CacheProfileLocally(PlayerProfile p_profile)
    {
        string jsonData = JsonSerializer.Serialize(p_profile, m_jsonOptions);
        m_saveService.SaveData(CACHE_FILE, jsonData);
    }

    private PlayerProfile? GetCachedProfile()
    {
        string jsonData = m_saveService.LoadData(CACHE_FILE);
        if (string.IsNullOrEmpty(jsonData)) return null;

        try
        {
            return JsonSerializer.Deserialize<PlayerProfile>(jsonData, m_jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private void UpdateLocalCache(SyncRequest p_request)
    {
        PlayerProfile profile = GetCachedProfile() ?? new PlayerProfile();

        if (p_request.Stats != null)
        {
            profile.Stats = p_request.Stats;
        }

        if (p_request.Config != null)
        {
            profile.Config = p_request.Config;
        }

        if (p_request.Inventory != null)
        {
            profile.Inventory = p_request.Inventory;
        }

        CacheProfileLocally(profile);
    }

    private class LoginResponse
    {
        public string? SessionToken { get; set; }
    }
}
