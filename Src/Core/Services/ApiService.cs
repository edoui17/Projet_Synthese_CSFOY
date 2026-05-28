using System;
using System.Collections.Generic;
using System.Linq;
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
    private const string SESSION_HEADER = "X-Session-Token";

    // We are going to use default options to handle circular ref just in case
    private readonly JsonSerializerOptions m_jsonOptions;

    public ApiService(ISaveService p_saveService, string p_apiKey)
    {
        m_httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5271")
        };
        m_httpClient.DefaultRequestHeaders.Add("X-API-KEY", p_apiKey);
        m_saveService = p_saveService;
        m_jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
    }

    public bool HasSessionToken => !string.IsNullOrEmpty(m_sessionToken);

    public void SetSessionToken(string? p_token)
    {
        m_sessionToken = p_token;
        if (m_httpClient.DefaultRequestHeaders.Contains(SESSION_HEADER))
        {
            m_httpClient.DefaultRequestHeaders.Remove(SESSION_HEADER);
        }

        if (!string.IsNullOrEmpty(m_sessionToken))
        {
            m_httpClient.DefaultRequestHeaders.Add(SESSION_HEADER, m_sessionToken);
        }
    }

    public async Task<string?> LoginAsync(string p_username, string p_password)
    {
        LoginRequest request = new LoginRequest { Username = p_username, Password = p_password };
        HttpResponseMessage response = await m_httpClient.PostAsJsonAsync("/api/auth/login", request);

        if (response.IsSuccessStatusCode)
        {
            LoginResponse? result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            SetSessionToken(result?.SessionToken);
            return m_sessionToken;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }

        // Throw for other errors (500, 503, etc.) to allow distinguishing from invalid credentials
        response.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<ProfileResponse?> GetProfileAsync()
    {
        if (!string.IsNullOrEmpty(m_sessionToken))
        {
            try
            {
                HttpResponseMessage response = await m_httpClient.GetAsync("/api/player/profile");

                if (response.IsSuccessStatusCode)
                {
                    ProfileResponse? profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(m_jsonOptions);
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
        // Update local cache to reflect current state
        UpdateLocalCache(p_request);

        if (string.IsNullOrEmpty(m_sessionToken))
        {
            // We are offline and don't have a token, we just rely on local cache updated above
            return false;
        }

        try
        {
            HttpResponseMessage response = await m_httpClient.PostAsJsonAsync("/api/player/sync", p_request, options: m_jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            // API down, offline fallback only
            return false;
        }
    }

    private void CacheProfileLocally(ProfileResponse p_profile)
    {
        string jsonData = JsonSerializer.Serialize(p_profile, m_jsonOptions);
        m_saveService.SaveData(CACHE_FILE, jsonData);
    }

    public ProfileResponse? GetCachedProfile()
    {
        string jsonData = m_saveService.LoadData(CACHE_FILE);
        if (string.IsNullOrEmpty(jsonData)) return null;

        try
        {
            return JsonSerializer.Deserialize<ProfileResponse>(jsonData, m_jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private void UpdateLocalCache(SyncRequest p_request)
    {
        ProfileResponse profile = GetCachedProfile() ?? new ProfileResponse();

        if (p_request.Stats != null)
        {
            var sessions = (profile.LastSessions ?? new List<GameStats>()).ToList();
            sessions.Add(p_request.Stats);
            profile.LastSessions = sessions;

            if (p_request.Stats.Score > profile.HighScore)
            {
                profile.HighScore = p_request.Stats.Score;
            }
        }

        if (p_request.Config != null)
        {
            profile.Config = p_request.Config;
        }

        if (p_request.Inventory != null)
        {
            profile.Inventory = p_request.Inventory?.ToList() ?? new List<InventoryEntry>();
        }

        CacheProfileLocally(profile);
    }

    private class LoginResponse
    {
        public string? SessionToken { get; set; }
    }
}
