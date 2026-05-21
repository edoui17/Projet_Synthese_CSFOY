using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext m_context;

    public InventoryRepository(AppDbContext p_context)
    {
        m_context = p_context;
    }

    public async Task<IEnumerable<InventoryEntry>> GetByPlayerIdAsync(Guid p_playerId)
    {
        var entities = await m_context.Inventory
            .Include(i => i.ResourceItem)
            .Where(i => i.PlayerId == p_playerId)
            .ToListAsync();

        return entities.Select(i => new InventoryEntry
        {
            PlayerId = i.PlayerId,
            ResourceItemId = i.ResourceItemId,
            Quantity = i.Quantity,
            ResourceItem = i.ResourceItem == null ? null : new ResourceItem
            {
                Id = i.ResourceItem.Id,
                Name = i.ResourceItem.Name,
                Type = i.ResourceItem.Type,
                IconPath = i.ResourceItem.IconPath
            }
        });
    }

    public async Task UpdateInventoryAsync(Guid p_playerId, IEnumerable<InventoryEntry> p_entries)
    {
        if (p_entries == null)
        {
            throw new ArgumentNullException(nameof(p_entries), "Inventory entries cannot be null.");
        }

        var existing = await m_context.Inventory.Where(i => i.PlayerId == p_playerId).ToListAsync();
        m_context.Inventory.RemoveRange(existing);

        var entities = p_entries.Select(e => new InventoryEntity
        {
            PlayerId = p_playerId,
            ResourceItemId = e.ResourceItemId,
            Quantity = e.Quantity
        });

        await m_context.Inventory.AddRangeAsync(entities);
        await m_context.SaveChangesAsync();
    }
}
