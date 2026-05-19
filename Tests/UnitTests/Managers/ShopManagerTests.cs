using Xunit;
using Core.Managers;
using Core.Interfaces;
using Core.Domain;

namespace UnitTests.Managers;

public class ShopManagerTests
{
    [Fact]
    public void CalculateCost_WithZeroUpgrades_ReturnsOne()
    {
        // Arrange
        IShopManager shopManager = new ShopManager(new global::Core.Services.EventBus());

        // Act
        int cost = shopManager.CalculateCost(0);

        // Assert
        Assert.Equal(1, cost);
    }

    [Fact]
    public void CalculateCost_WithOneUpgrade_ReturnsThree()
    {
        // Arrange
        IShopManager shopManager = new ShopManager(new global::Core.Services.EventBus());

        // Act
        int cost = shopManager.CalculateCost(1);

        // Assert
        Assert.Equal(3, cost);
    }

    [Fact]
    public void CalculateCost_WithTwoUpgrades_ReturnsSeven()
    {
        // Arrange
        IShopManager shopManager = new ShopManager(new global::Core.Services.EventBus());

        // Act
        int cost = shopManager.CalculateCost(2);

        // Assert
        Assert.Equal(7, cost);
    }

    [Fact]
    public void CanAffordUpgrade_WithEnoughResources_ReturnsTrue()
    {
        // Arrange
        IShopManager shopManager = new ShopManager(new global::Core.Services.EventBus());
        IInventoryManager inventoryManager = new InventoryManager(new global::Core.Services.EventBus());
        inventoryManager.AddMaterial(new ResourceItem("res1", "Viande", "Food", ""), 5);

        // Act
        bool result = shopManager.CanAffordUpgrade(inventoryManager, "res1", 3);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAffordUpgrade_WithInsufficientResources_ReturnsFalse()
    {
        // Arrange
        IShopManager shopManager = new ShopManager(new global::Core.Services.EventBus());
        IInventoryManager inventoryManager = new InventoryManager(new global::Core.Services.EventBus());
        inventoryManager.AddMaterial(new ResourceItem("res1", "Viande", "Food", ""), 2);

        // Act
        bool result = shopManager.CanAffordUpgrade(inventoryManager, "res1", 3);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanAffordIsland_WithEnoughOfAllResources_ReturnsTrue()
    {
        // Arrange
        IShopManager shopManager = new ShopManager(new global::Core.Services.EventBus());
        IInventoryManager inventoryManager = new InventoryManager(new global::Core.Services.EventBus());

        inventoryManager.AddMaterial(new ResourceItem("meat_01", "meat_01", "Food", ""), 1);
        inventoryManager.AddMaterial(new ResourceItem("wood_01", "wood_01", "Material", ""), 1);
        inventoryManager.AddMaterial(new ResourceItem("rock_01", "rock_01", "Material", ""), 1);
        inventoryManager.AddMaterial(new ResourceItem("gold_01", "gold_01", "Currency", ""), 1);

        // Act
        bool result = shopManager.CanAffordIsland(inventoryManager, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAffordIsland_WithInsufficientOfOneResource_ReturnsFalse()
    {
        // Arrange
        IShopManager shopManager = new ShopManager(new global::Core.Services.EventBus());
        IInventoryManager inventoryManager = new InventoryManager(new global::Core.Services.EventBus());

        inventoryManager.AddMaterial(new ResourceItem("meat_01", "meat_01", "Food", ""), 1);
        inventoryManager.AddMaterial(new ResourceItem("wood_01", "wood_01", "Material", ""), 1);
        inventoryManager.AddMaterial(new ResourceItem("rock_01", "rock_01", "Material", ""), 1);
        // Missing "gold_01"

        // Act
        bool result = shopManager.CanAffordIsland(inventoryManager, 1);

        // Assert
        Assert.False(result);
    }
}