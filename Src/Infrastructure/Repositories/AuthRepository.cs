using System;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext m_context;

    public AuthRepository(AppDbContext p_context)
    {
        m_context = p_context;
    }

    public async Task<Player?> GetBySessionTokenAsync(string p_token)
    {
        var entity = await m_context.Players
            .Include(p => p.Stats)
            .Include(p => p.Config)
            .FirstOrDefaultAsync(p => p.SessionToken == p_token);

        return MapToDomain(entity);
    }

    public async Task UpdateSessionTokenAsync(Guid p_playerId, string? p_token)
    {
        var entity = await m_context.Players.FindAsync(p_playerId);
        if (entity != null)
        {
            entity.SessionToken = p_token;
            await m_context.SaveChangesAsync();
        }
    }

    public async Task<bool> VerifyPasswordAsync(string p_username, string p_password)
    {
        var entity = await m_context.Players.FirstOrDefaultAsync(p => p.Username == p_username);
        if (entity == null) return false;

        return entity.PasswordHash == p_password;
    }

    private Player? MapToDomain(PlayerEntity? p_entity)
    {
        if (p_entity == null) return null;

        return new Player
        {
            Id = p_entity.Id,
            Username = p_entity.Username,
            PasswordHash = p_entity.PasswordHash,
            SessionToken = p_entity.SessionToken,
            CreatedAt = p_entity.CreatedAt,
            Stats = p_entity.Stats == null ? null : new PlayerStats
            {
                PlayerId = p_entity.Stats.PlayerId,
                Health = p_entity.Stats.Health,
                Attack = p_entity.Stats.Attack,
                Speed = p_entity.Stats.Speed,
                Luck = p_entity.Stats.Luck,
                ExtraStats = p_entity.Stats.ExtraStats
            },
            Config = p_entity.Config == null ? null : new PlayerConfig
            {
                PlayerId = p_entity.Config.PlayerId,
                MasterVolume = p_entity.Config.MasterVolume,
                MusicVolume = p_entity.Config.MusicVolume,
                SfxVolume = p_entity.Config.SfxVolume
            }
        };
    }
}
