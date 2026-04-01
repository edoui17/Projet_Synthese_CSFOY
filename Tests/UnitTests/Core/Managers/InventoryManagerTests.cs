using Core.Domain;
using Core.Managers;
using Xunit;

namespace UnitTests.Core.Managers
{
    public class InventoryManagerTests
    {
        [Fact]
        public void AddMaterial_WithNewItem_ShouldCreateSlot()
        {
            // Arrange
            var manager = new InventoryManager();
            var item = new ResourceItem("gold_01", "Or", "Gold", "res://icon.png");

            // Act
            manager.AddMaterial(item, 5);

            // Assert
            Assert.Equal(5, manager.GetMaterialCount("gold_01"));
            Assert.Single(manager.GetAllSlots());
            Assert.Equal("Or", manager.GetAllSlots()[0].Item.Name);
        }

        [Fact]
        public void AddMaterial_WithExistingItem_ShouldIncrementQuantity()
        {
            // Arrange
            var manager = new InventoryManager();
            var item = new ResourceItem("wood_01", "Bois", "Wood", "res://wood.png");

            // Act
            manager.AddMaterial(item, 10);
            manager.AddMaterial(item, 5);

            // Assert
            Assert.Equal(15, manager.GetMaterialCount("wood_01"));
            Assert.Single(manager.GetAllSlots());
        }

        [Fact]
        public void RemoveMaterial_WithValidAmount_ShouldDecrementQuantity()
        {
            // Arrange
            var manager = new InventoryManager();
            var item = new ResourceItem("rock_01", "Roche", "Rock", "res://rock.png");
            manager.AddMaterial(item, 10);

            // Act
            manager.RemoveMaterial("rock_01", 3);

            // Assert
            Assert.Equal(7, manager.GetMaterialCount("rock_01"));
        }

        [Fact]
        public void RemoveMaterial_WithAmountGreaterThanStock_ShouldRemoveSlot()
        {
            // Arrange
            var manager = new InventoryManager();
            var item = new ResourceItem("rock_01", "Roche", "Rock", "res://rock.png");
            manager.AddMaterial(item, 5);

            // Act
            manager.RemoveMaterial("rock_01", 10);

            // Assert
            Assert.Equal(0, manager.GetMaterialCount("rock_01"));
            Assert.Empty(manager.GetAllSlots());
        }

        [Fact]
        public void GetMaterialCount_WithNonExistentItem_ShouldReturnZero()
        {
            // Arrange
            var manager = new InventoryManager();

            // Act
            int count = manager.GetMaterialCount("invalid_id");

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public void GetAllSlots_ShouldReturnReadOnlyCopy()
        {
            // Arrange
            var manager = new InventoryManager();
            var item1 = new ResourceItem("gold_01", "Or", "Gold", "res://gold.png");
            var item2 = new ResourceItem("rock_01", "Roche", "Rock", "res://rock.png");

            manager.AddMaterial(item1, 5);
            manager.AddMaterial(item2, 10);

            // Act
            var slots = manager.GetAllSlots();

            // Assert
            Assert.Equal(2, slots.Count);
            Assert.Contains(slots, s => s.Item.Id == "gold_01" && s.Quantity == 5);
            Assert.Contains(slots, s => s.Item.Id == "rock_01" && s.Quantity == 10);
        }
    }
}
