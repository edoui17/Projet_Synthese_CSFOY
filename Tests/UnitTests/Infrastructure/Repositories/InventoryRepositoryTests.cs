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
    public async Task GetByPlayerIdAsync_WhenPlayerHasNoInventory_ReturnsEmptyList()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var playerId = Guid.NewGuid();

        using (var context = GetInMemoryDbContext(dbName))
        {
            var repository = new InventoryRepository(context);

            // Act
            var result = await repository.GetByPlayerIdAsync(playerId);

            // Assert
            Assert.Empty(result);
        }
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WhenPlayerHasInventory_ReturnsOnlyThatPlayersInventory()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var playerId = Guid.NewGuid();
        var otherPlayerId = Guid.NewGuid();

        using (var context = GetInMemoryDbContext(dbName))
        {
            context.Players.Add(new PlayerEntity { Id = playerId, Username = "player1" });
            context.Players.Add(new PlayerEntity { Id = otherPlayerId, Username = "player2" });
            context.ResourceItems.Add(new ResourceItemEntity { Id = "wood_01", Name = "Wood", Type = "Material" });
            context.ResourceItems.Add(new ResourceItemEntity { Id = "stone_01", Name = "Stone", Type = "Material" });

            context.Inventory.Add(new InventoryEntity { PlayerId = playerId, ResourceItemId = "wood_01", Quantity = 10 });
            context.Inventory.Add(new InventoryEntity { PlayerId = otherPlayerId, ResourceItemId = "stone_01", Quantity = 5 });
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryDbContext(dbName))
        {
            var repository = new InventoryRepository(context);

            // Act
            var result = await repository.GetByPlayerIdAsync(playerId);

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal(playerId, list[0].PlayerId);
            Assert.Equal("wood_01", list[0].ResourceItemId);
            Assert.Equal(10, list[0].Quantity);
        }
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WhenResourceItemIsNull_MapsCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var playerId = Guid.NewGuid();

        using (var context = GetInMemoryDbContext(dbName))
        {
            context.Players.Add(new PlayerEntity { Id = playerId, Username = "player1" });

            // To test the null mapping logic defensively in the repository we can just
            // add a valid resource item, but null out its values or remove it.
            // However, due to InMemory EF core using an INNER JOIN for Include on required foreign keys,
            // we will instead ensure the logic for normal mapping is heavily tested.
            // A separate test "GetByPlayerIdAsync_WhenResourceItemIsNotNull_MapsCorrectly" covers the normal branch.

            // For the null branch, if it's strictly required by coverage, we can simulate an orphaned row
            // by clearing the navigation property AFTER loading it, or by realizing the in-memory DB
            // can sometimes return null for optional relationships.
            // Since `InventoryEntity.ResourceItem` is technically optional in the class definition (`ResourceItemEntity?`),
            // but the foreign key (`ResourceItemId`) is a non-nullable string.

            // We can trick EF by saving a ResourceItem, and then we manually change the ResourceItem to null
            // on the entity instance in the ChangeTracker before calling the method,
            // so when `ToListAsync()` executes on the same context instance, it yields the tracked entity
            // with the nullified navigation property.

            var resource = new ResourceItemEntity { Id = "wood_01", Name = "Wood", Type = "Material" };
            var inventory = new InventoryEntity { PlayerId = playerId, ResourceItemId = "wood_01", Quantity = 10, ResourceItem = resource };

            context.ResourceItems.Add(resource);
            context.Inventory.Add(inventory);
            await context.SaveChangesAsync();

            // Now nullify the navigation property in the tracked entity
            inventory.ResourceItem = null;

            var repository = new InventoryRepository(context);

            // Act
            // Because the entity is tracked, ToListAsync() will return the tracked instance,
            // which we just mutated to have a null ResourceItem.
            var result = await repository.GetByPlayerIdAsync(playerId);

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal(playerId, list[0].PlayerId);
            Assert.Equal("wood_01", list[0].ResourceItemId);
            Assert.Equal(10, list[0].Quantity);
            Assert.Null(list[0].ResourceItem);
        }
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WhenResourceItemIsNotNull_MapsCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var playerId = Guid.NewGuid();

        using (var context = GetInMemoryDbContext(dbName))
        {
            context.Players.Add(new PlayerEntity { Id = playerId, Username = "player1" });
            var resourceItem = new ResourceItemEntity { Id = "wood_01", Name = "Wood", Type = "Material", IconPath = "path/to/icon" };
            context.ResourceItems.Add(resourceItem);
            context.Inventory.Add(new InventoryEntity { PlayerId = playerId, ResourceItemId = "wood_01", Quantity = 10, ResourceItem = resourceItem });
            await context.SaveChangesAsync();
        }

        using (var context = GetInMemoryDbContext(dbName))
        {
            var repository = new InventoryRepository(context);

            // Act
            var result = await repository.GetByPlayerIdAsync(playerId);

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal(playerId, list[0].PlayerId);
            Assert.Equal("wood_01", list[0].ResourceItemId);
            Assert.Equal(10, list[0].Quantity);
            Assert.NotNull(list[0].ResourceItem);
            Assert.Equal("wood_01", list[0].ResourceItem!.Id);
            Assert.Equal("Wood", list[0].ResourceItem!.Name);
            Assert.Equal("Material", list[0].ResourceItem!.Type);
            Assert.Equal("path/to/icon", list[0].ResourceItem!.IconPath);
        }
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
