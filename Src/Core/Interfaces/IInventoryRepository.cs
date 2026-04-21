using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Interfaces;

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryEntry>> GetByPlayerIdAsync(Guid p_playerId);
    Task UpdateInventoryAsync(Guid p_playerId, IEnumerable<InventoryEntry> p_entries);
}
