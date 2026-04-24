using System;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces;

public interface IAuthRepository
{
    Task<Player?> GetBySessionTokenAsync(string p_token);
    Task UpdateSessionTokenAsync(Guid p_playerId, string? p_token);
    Task<bool> VerifyPasswordAsync(string p_username, string p_password);
}
