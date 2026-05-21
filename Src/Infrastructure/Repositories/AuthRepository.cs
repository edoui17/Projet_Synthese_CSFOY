using System;
using System.Linq;
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
            .Include(p => p.Config)
            //===========================================================================
            .Include(p => p.GameStats)
            //===========================================================================
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
            UpdatedAt = p_entity.UpdatedAt,
            HighScore = p_entity.HighScore,
            GameStats = p_entity.GameStats.Select(s => new GameStats
            {
                Id = s.Id,
                PlayerId = s.PlayerId,
                PlayedAt = s.PlayedAt,
                Duration = s.Duration,
                LevelReached = s.LevelReached,
                Score = s.Score,
                Health = s.Health,
                Attack = s.Attack,
                Speed = s.Speed,
                Luck = s.Luck,
                BonusHealth = s.BonusHealth,
                BonusAttack = s.BonusAttack,
                BonusSpeed = s.BonusSpeed,
                BonusLuck = s.BonusLuck,
                ExtraStats = s.ExtraStats
            }).ToList(),
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
