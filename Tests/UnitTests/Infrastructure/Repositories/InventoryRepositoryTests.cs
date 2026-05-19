using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using Infrastructure;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.UnitTests.Infrastructure.Repositories;

public class InventoryRepositoryTests
{
    private AppDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task UpdateInventoryAsync_WithValidEntries_ReplacesExistingInventory()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var playerId = Guid.NewGuid();

        using (var context = GetInMemoryDbContext(dbName))
        {
            context.Inventory.Add(new InventoryEntity { PlayerId = playerId, ResourceItemId = "wood_01", Quantity = 10 });
            context.Inventory.Add(new InventoryEntity { PlayerId = playerId, ResourceItemId = "stone_01", Quantity = 5 });
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryDbContext(dbName))
        {
            var repository = new InventoryRepository(context);
            var newEntries = new List<InventoryEntry>
            {
                new InventoryEntry { PlayerId = playerId, ResourceItemId = "wood_01", Quantity = 15 },
                new InventoryEntry { PlayerId = playerId, ResourceItemId = "gold_01", Quantity = 2 }
            };

            // Act
            await repository.UpdateInventoryAsync(playerId, newEntries);
        }

        // Assert
        using (var context = GetInMemoryDbContext(dbName))
        {
            var inventory = await context.Inventory.Where(i => i.PlayerId == playerId).ToListAsync();

            Assert.Equal(2, inventory.Count);

            var wood = inventory.FirstOrDefault(i => i.ResourceItemId == "wood_01");
            Assert.NotNull(wood);
            Assert.Equal(15, wood.Quantity);

            var gold = inventory.FirstOrDefault(i => i.ResourceItemId == "gold_01");
            Assert.NotNull(gold);
            Assert.Equal(2, gold.Quantity);

            var stone = inventory.FirstOrDefault(i => i.ResourceItemId == "stone_01");
            Assert.Null(stone); // Should have been removed
        }
    }

    [Fact]
    public async Task UpdateInventoryAsync_WithEmptyEntries_RemovesAllInventory()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var playerId = Guid.NewGuid();

        using (var context = GetInMemoryDbContext(dbName))
        {
            context.Inventory.Add(new InventoryEntity { PlayerId = playerId, ResourceItemId = "wood_01", Quantity = 10 });
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryDbContext(dbName))
        {
            var repository = new InventoryRepository(context);
            var emptyEntries = new List<InventoryEntry>();

            // Act
            await repository.UpdateInventoryAsync(playerId, emptyEntries);
        }

        // Assert
        using (var context = GetInMemoryDbContext(dbName))
        {
            var inventory = await context.Inventory.Where(i => i.PlayerId == playerId).ToListAsync();
            Assert.Empty(inventory);
        }
    }

    [Fact]
    public async Task UpdateInventoryAsync_NullEntries_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var playerId = Guid.NewGuid();

        using (var context = GetInMemoryDbContext(dbName))
        {
            var repository = new InventoryRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateInventoryAsync(playerId, null!));
        }
    }

    [Fact]
    public async Task UpdateInventoryAsync_OnlyModifiesTargetPlayer()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        using (var context = GetInMemoryDbContext(dbName))
        {
            context.Inventory.Add(new InventoryEntity { PlayerId = player1Id, ResourceItemId = "wood_01", Quantity = 10 });
            context.Inventory.Add(new InventoryEntity { PlayerId = player2Id, ResourceItemId = "stone_01", Quantity = 5 });
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryDbContext(dbName))
        {
            var repository = new InventoryRepository(context);
            var newEntries = new List<InventoryEntry>
            {
                new InventoryEntry { PlayerId = player1Id, ResourceItemId = "wood_01", Quantity = 20 }
            };

            // Act
            await repository.UpdateInventoryAsync(player1Id, newEntries);
        }

        // Assert
        using (var context = GetInMemoryDbContext(dbName))
        {
            // Player 1 should be updated
            var player1Inventory = await context.Inventory.Where(i => i.PlayerId == player1Id).ToListAsync();
            Assert.Single(player1Inventory);
            Assert.Equal(20, player1Inventory[0].Quantity);

            // Player 2 should remain unchanged
            var player2Inventory = await context.Inventory.Where(i => i.PlayerId == player2Id).ToListAsync();
            Assert.Single(player2Inventory);
            Assert.Equal(5, player2Inventory[0].Quantity);
        }
    }
}
