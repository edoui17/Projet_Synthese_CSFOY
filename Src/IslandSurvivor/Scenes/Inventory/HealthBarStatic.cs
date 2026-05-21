using Godot;
using System;
using Core.Managers.Stats;

[Tool]
public partial class HealthBarStatic : Control
{
    [Export] public TextureProgressBar ProgressBar = null!;
    [Export] public Label HealthLabel = null!;

    private IslandSurvivor.Nodes.StatManager m_playerStats = null!;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        CallDeferred(nameof(InitializePlayerConnection));
    }

    private void InitializePlayerConnection()
    {
        // Find player and their StatManager
        var player = GetTree().GetFirstNodeInGroup("Player") as Node;
        if (player != null)
        {
            m_playerStats = player.GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");

            if (m_playerStats != null)
            {
                float maxHealth = m_playerStats.GetEffectiveMaxValue(StatType.Health);
                float currentHealth = m_playerStats.GetCurrentValue(StatType.Health);
                UpdateHealthUI(currentHealth, maxHealth);

                m_playerStats.Connect(IslandSurvivor.Nodes.StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
            }
        }
    }

    private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        if ((StatType)p_statType == StatType.Health)
        {
            UpdateHealthUI(p_currentValue, p_effectiveMaxValue);
        }
    }

    private void UpdateHealthUI(float p_currentHealth, float p_maxHealth)
    {
        if (ProgressBar != null)
        {
            ProgressBar.MaxValue = p_maxHealth;
            ProgressBar.Value = p_currentHealth;
        }

        if (HealthLabel != null)
        {
            HealthLabel.Text = $"{(int)p_currentHealth} / {(int)p_maxHealth}";
        }
    }

    protected override void Dispose(bool p_disposing)
    {
        if (p_disposing && m_playerStats != null && !Engine.IsEditorHint())
        {
            m_playerStats.Disconnect(IslandSurvivor.Nodes.StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
        base.Dispose(p_disposing);
    }
}
