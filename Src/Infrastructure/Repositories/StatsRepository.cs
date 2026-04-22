using System;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StatsRepository : IStatsRepository
{
    private readonly AppDbContext m_context;

    public StatsRepository(AppDbContext p_context)
    {
        m_context = p_context;
    }

    public async Task<PlayerStats?> GetByPlayerIdAsync(Guid p_playerId)
    {
        var entity = await m_context.Stats.FirstOrDefaultAsync(s => s.PlayerId == p_playerId);
        if (entity == null) return null;

        return new PlayerStats
        {
            PlayerId = entity.PlayerId,
            Health = entity.Health,
            Attack = entity.Attack,
            Speed = entity.Speed,
            Luck = entity.Luck,
            ExtraStats = entity.ExtraStats
        };
    }

    public async Task UpdateStatsAsync(PlayerStats p_stats)
    {
        var entity = await m_context.Stats.FirstOrDefaultAsync(s => s.PlayerId == p_stats.PlayerId);
        if (entity == null)
        {
            entity = new StatsEntity { PlayerId = p_stats.PlayerId };
            await m_context.Stats.AddAsync(entity);
        }

        entity.Health = p_stats.Health;
        entity.Attack = p_stats.Attack;
        entity.Speed = p_stats.Speed;
        entity.Luck = p_stats.Luck;
        entity.ExtraStats = p_stats.ExtraStats;

        await m_context.SaveChangesAsync();
    }
}
