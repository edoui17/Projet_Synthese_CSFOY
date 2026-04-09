namespace Core.Interfaces;

using System;

public interface IHealthComponent
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsDead { get; }

    event EventHandler<int> OnDamageTaken;
    event EventHandler OnDeath;

    void TakeDamage(int p_amount);
    void Heal(int p_amount);
}
