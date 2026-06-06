using System;
using Xunit;
using Core.Interfaces;

namespace UnitTests.Core.Stats
{
    // A simple fake implementation of IDamageable for testing purposes without using Moq
    public class FakeDamageableEntity : IDamageable
    {
        public string NpcType { get; set; } = "FakeNpc";
        public int Health { get; private set; }
        public bool IsDead => Health <= 0;
        public object? LastAttacker { get; private set; }

        public FakeDamageableEntity(int initialHealth)
        {
            Health = initialHealth;
        }

        public void TakeDamage(int p_amount, object p_attacker)
        {
            if (p_amount < 0) return; // Ignore negative damage

            Health -= p_amount;
            if (Health < 0) Health = 0; // Prevent health from dropping below 0

            LastAttacker = p_attacker;
        }
    }

    public class IDamageableTests
    {
        [Fact]
        public void TakeDamage_ShouldDeductHealthCorrectly()
        {
            // Arrange
            var entity = new FakeDamageableEntity(100);
            var attacker = new object();

            // Act
            entity.TakeDamage(20, attacker);

            // Assert
            Assert.Equal(80, entity.Health);
            Assert.False(entity.IsDead);
            Assert.Equal(attacker, entity.LastAttacker);
        }

        [Fact]
        public void TakeDamage_ShouldNotAllowHealthToDropBelowZero()
        {
            // Arrange
            var entity = new FakeDamageableEntity(50);
            var attacker = new object();

            // Act
            entity.TakeDamage(100, attacker);

            // Assert
            Assert.Equal(0, entity.Health);
            Assert.True(entity.IsDead);
            Assert.Equal(attacker, entity.LastAttacker);
        }

        [Fact]
        public void TakeDamage_ShouldHandleExactLethalDamage()
        {
            // Arrange
            var entity = new FakeDamageableEntity(30);
            var attacker = new object();

            // Act
            entity.TakeDamage(30, attacker);

            // Assert
            Assert.Equal(0, entity.Health);
            Assert.True(entity.IsDead);
        }

        [Fact]
        public void TakeDamage_ShouldIgnoreNegativeDamage()
        {
            // Arrange
            var entity = new FakeDamageableEntity(50);
            var attacker = new object();

            // Act
            entity.TakeDamage(-10, attacker);

            // Assert
            Assert.Equal(50, entity.Health);
        }
    }
}