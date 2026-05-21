using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces;

public interface IStatsRepository
{
    Task<IEnumerable<GameStats>> GetByPlayerIdAsync(Guid p_playerId);
    Task<IEnumerable<GameStats>> GetTopStatsByPlayerIdAsync(Guid p_playerId, int p_count = 10);
    Task AddGameStatsAsync(GameStats p_stats);
}
