using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces;

public interface IApiService
{
    bool HasSessionToken { get; }
    void SetSessionToken(string? p_token);
    Task<string?> LoginAsync(string p_username, string p_password);
    Task<ProfileResponse?> GetProfileAsync();
    ProfileResponse? GetCachedProfile();
    Task<bool> SyncAsync(SyncRequest p_request);
}
