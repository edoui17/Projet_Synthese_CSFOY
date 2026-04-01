using Core.Domain;
using Core.Managers;

namespace UnitTests.Managers;

public class InventoryManagerTests
{
    [Fact]
    public void AddMaterial_WithValidItem_IncreasesQuantity()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item = new ResourceItem("wood", "Wood", "Resource", "path/to/icon");

        // Act
        inventoryManager.AddMaterial(item, 5);

        // Assert
        Assert.Equal(5, inventoryManager.GetMaterialCount("wood"));
    }

    [Fact]
    public void AddMaterial_SameItemMultipleTimes_AccumulatesQuantity()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item = new ResourceItem("stone", "Stone", "Resource", "path/to/icon");

        // Act
        inventoryManager.AddMaterial(item, 3);
        inventoryManager.AddMaterial(item, 4);

        // Assert
        Assert.Equal(7, inventoryManager.GetMaterialCount("stone"));
    }

    [Fact]
    public void RemoveMaterial_WithValidAmount_DecreasesQuantity()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item = new ResourceItem("iron", "Iron", "Resource", "path/to/icon");
        inventoryManager.AddMaterial(item, 10);

        // Act
        inventoryManager.RemoveMaterial("iron", 4);

        // Assert
        Assert.Equal(6, inventoryManager.GetMaterialCount("iron"));
    }

    [Fact]
    public void RemoveMaterial_ExactAmount_RemovesItemFromInventory()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item = new ResourceItem("gold", "Gold", "Resource", "path/to/icon");
        inventoryManager.AddMaterial(item, 5);

        // Act
        inventoryManager.RemoveMaterial("gold", 5);

        // Assert
        Assert.Equal(0, inventoryManager.GetMaterialCount("gold"));
        Assert.Empty(inventoryManager.GetAllSlots());
    }

    [Fact]
    public void GetAllSlots_ReturnsCorrectNumberOfSlots()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item1 = new ResourceItem("wood", "Wood", "Resource", "path/to/icon");
        var item2 = new ResourceItem("stone", "Stone", "Resource", "path/to/icon");

        // Act
        inventoryManager.AddMaterial(item1, 5);
        inventoryManager.AddMaterial(item2, 10);

        // Assert
        var slots = inventoryManager.GetAllSlots();
        Assert.Equal(2, slots.Count);
        Assert.Contains(slots, s => s.Item.Id == "wood" && s.Quantity == 5);
        Assert.Contains(slots, s => s.Item.Id == "stone" && s.Quantity == 10);
    }

    [Fact]
    public void AddMaterial_WithNullItem_DoesNothing()
    {
        // Arrange
        var inventoryManager = new InventoryManager();

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        inventoryManager.AddMaterial(null, 5);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Assert
        Assert.Empty(inventoryManager.GetAllSlots());
    }

    [Fact]
    public void AddMaterial_WithEmptyItemId_DoesNothing()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item = new ResourceItem("", "Wood", "Resource", "path/to/icon");

        // Act
        inventoryManager.AddMaterial(item, 5);

        // Assert
        Assert.Empty(inventoryManager.GetAllSlots());
    }

    [Fact]
    public void AddMaterial_WithZeroOrNegativeAmount_DoesNothing()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item = new ResourceItem("wood", "Wood", "Resource", "path/to/icon");

        // Act
        inventoryManager.AddMaterial(item, 0);
        inventoryManager.AddMaterial(item, -5);

        // Assert
        Assert.Empty(inventoryManager.GetAllSlots());
    }

    [Fact]
    public void RemoveMaterial_WithZeroOrNegativeAmount_DoesNothing()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item = new ResourceItem("wood", "Wood", "Resource", "path/to/icon");
        inventoryManager.AddMaterial(item, 10);

        // Act
        inventoryManager.RemoveMaterial("wood", 0);
        inventoryManager.RemoveMaterial("wood", -5);

        // Assert
        Assert.Equal(10, inventoryManager.GetMaterialCount("wood"));
    }

    [Fact]
    public void RemoveMaterial_MoreThanAvailable_RemovesItemFromInventory()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item = new ResourceItem("wood", "Wood", "Resource", "path/to/icon");
        inventoryManager.AddMaterial(item, 5);

        // Act
        inventoryManager.RemoveMaterial("wood", 10);

        // Assert
        Assert.Equal(0, inventoryManager.GetMaterialCount("wood"));
        Assert.Empty(inventoryManager.GetAllSlots());
    }

    [Fact]
    public void RemoveMaterial_NonExistentItem_DoesNothing()
    {
        // Arrange
        var inventoryManager = new InventoryManager();
        var item = new ResourceItem("wood", "Wood", "Resource", "path/to/icon");
        inventoryManager.AddMaterial(item, 5);

        // Act
        inventoryManager.RemoveMaterial("stone", 5);

        // Assert
        Assert.Equal(5, inventoryManager.GetMaterialCount("wood"));
        var slots = inventoryManager.GetAllSlots();
        Assert.Single(slots);
    }

    [Fact]
    public void GetMaterialCount_WithNullOrEmptyId_ReturnsZero()
    {
        // Arrange
        var inventoryManager = new InventoryManager();

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var countNull = inventoryManager.GetMaterialCount(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        var countEmpty = inventoryManager.GetMaterialCount("");

        // Assert
        Assert.Equal(0, countNull);
        Assert.Equal(0, countEmpty);
    }

    [Fact]
    public void GetMaterialCount_WithNonExistentId_ReturnsZero()
    {
        // Arrange
        var inventoryManager = new InventoryManager();

        // Act
        var count = inventoryManager.GetMaterialCount("nonexistent");

        // Assert
        Assert.Equal(0, count);
    }
}
