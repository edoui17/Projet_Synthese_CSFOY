namespace IslandSurvivor.Logic.Entities;

using System;
using IslandSurvivor.Interfaces;

public class HealthComponent : IHealthComponent
{
    private int m_currentHealth;

    public int CurrentHealth => m_currentHealth;
    public int MaxHealth { get; private set; }
    public bool IsDead => m_currentHealth <= 0;

    public event EventHandler<int> OnDamageTaken;
    public event EventHandler OnDeath;

    public HealthComponent(int p_maxHealth)
    {
        MaxHealth = p_maxHealth;
        m_currentHealth = p_maxHealth;
    }

    public void TakeDamage(int p_amount)
    {
        if (IsDead || p_amount <= 0) return;

        m_currentHealth -= p_amount;
        if (m_currentHealth < 0) m_currentHealth = 0;

        OnDamageTaken?.Invoke(this, p_amount);

        if (IsDead)
        {
            OnDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Heal(int p_amount)
    {
        if (IsDead || p_amount <= 0) return;

        m_currentHealth += p_amount;
        if (m_currentHealth > MaxHealth) m_currentHealth = MaxHealth;
    }
}
