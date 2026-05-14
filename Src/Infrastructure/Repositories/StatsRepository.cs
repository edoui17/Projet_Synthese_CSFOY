using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<IEnumerable<GameStats>> GetByPlayerIdAsync(Guid p_playerId)
    {
        var entities = await m_context.GameStats
            .Where(s => s.PlayerId == p_playerId)
            .OrderByDescending(s => s.PlayedAt)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task<IEnumerable<GameStats>> GetTopStatsByPlayerIdAsync(Guid p_playerId, int p_count = 10)
    {
        var entities = await m_context.GameStats
            .Where(s => s.PlayerId == p_playerId)
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.PlayedAt)
            .Take(p_count)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task AddGameStatsAsync(GameStats p_stats)
    {
        var entity = new GameStatsEntity
        {
            PlayerId = p_stats.PlayerId,
            PlayedAt = p_stats.PlayedAt,
            Duration = p_stats.Duration,
            LevelReached = p_stats.LevelReached,
            Score = p_stats.Score,
            Health = p_stats.Health,
            Attack = p_stats.Attack,
            Speed = p_stats.Speed,
            Luck = p_stats.Luck,
            BonusHealth = p_stats.BonusHealth,
            BonusAttack = p_stats.BonusAttack,
            BonusSpeed = p_stats.BonusSpeed,
            BonusLuck = p_stats.BonusLuck,
            ExtraStats = p_stats.ExtraStats
        };

        await m_context.GameStats.AddAsync(entity);
        await m_context.SaveChangesAsync();
    }

    private GameStats MapToDomain(GameStatsEntity p_entity)
    {
        return new GameStats
        {
            Id = p_entity.Id,
            PlayerId = p_entity.PlayerId,
            PlayedAt = p_entity.PlayedAt,
            Duration = p_entity.Duration,
            LevelReached = p_entity.LevelReached,
            Score = p_entity.Score,
            Health = p_entity.Health,
            Attack = p_entity.Attack,
            Speed = p_entity.Speed,
            Luck = p_entity.Luck,
            BonusHealth = p_entity.BonusHealth,
            BonusAttack = p_entity.BonusAttack,
            BonusSpeed = p_entity.BonusSpeed,
            BonusLuck = p_entity.BonusLuck,
            ExtraStats = p_entity.ExtraStats
        };
    }
}
