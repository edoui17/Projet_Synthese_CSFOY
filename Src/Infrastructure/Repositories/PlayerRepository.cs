using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly AppDbContext m_context;

    public PlayerRepository(AppDbContext p_context)
    {
        m_context = p_context;
    }

    public async Task<Player?> GetByIdAsync(object p_id)
    {
        if (p_id is Guid guidId)
        {
            var entity = await m_context.Players
                .Include(p => p.Stats)
                .Include(p => p.Config)
                .FirstOrDefaultAsync(p => p.Id == guidId);

            return MapToDomain(entity);
        }
        return null;
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        var entities = await m_context.Players.ToListAsync();
        return entities.Select(MapToDomain).Where(p => p != null)!;
    }

    public async Task AddAsync(Player p_entity)
    {
        var entity = new PlayerEntity
        {
            Id = p_entity.Id,
            Username = p_entity.Username,
            CreatedAt = p_entity.CreatedAt
        };
        await m_context.Players.AddAsync(entity);
    }

    public void Update(Player p_entity)
    {
        var entity = m_context.Players.Find(p_entity.Id);
        if (entity != null)
        {
            entity.Username = p_entity.Username;
            m_context.Players.Update(entity);
        }
    }

    public void Delete(Player p_entity)
    {
        var entity = m_context.Players.Find(p_entity.Id);
        if (entity != null)
        {
            m_context.Players.Remove(entity);
        }
    }

    public async Task SaveChangesAsync()
    {
        await m_context.SaveChangesAsync();
    }

    public async Task<Player?> GetByUsernameAsync(string p_username)
    {
        var entity = await m_context.Players
            .Include(p => p.Stats)
            .Include(p => p.Config)
            .FirstOrDefaultAsync(p => p.Username == p_username);

        return MapToDomain(entity);
    }

    private Player? MapToDomain(PlayerEntity? p_entity)
    {
        if (p_entity == null) return null;

        return new Player
        {
            Id = p_entity.Id,
            Username = p_entity.Username,
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
