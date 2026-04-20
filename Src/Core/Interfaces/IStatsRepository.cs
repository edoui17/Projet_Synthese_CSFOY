using System;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces;

public interface IStatsRepository
{
    Task<PlayerStats?> GetByPlayerIdAsync(Guid p_playerId);
    Task UpdateStatsAsync(PlayerStats p_stats);
}
