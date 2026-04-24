using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces;

public interface IApiService
{
    Task<string?> LoginAsync(string p_username, string p_password);
    Task<PlayerProfile?> GetProfileAsync();
    Task<bool> SyncAsync(SyncRequest p_request);
}
