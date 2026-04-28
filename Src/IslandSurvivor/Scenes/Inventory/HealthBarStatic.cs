using Godot;
using System;
using Core.Managers.Stats;

[Tool]
public partial class HealthBarStatic : Control
{
    [Export] public TextureProgressBar ProgressBar;
    [Export] public Label HealthLabel;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        // Fetch initial values
        if (IslandSurvivor.Globals.ServiceRegistry.Instance != null && IslandSurvivor.Globals.ServiceRegistry.Instance.StatTracker != null)
        {
            float maxHealth = IslandSurvivor.Globals.ServiceRegistry.Instance.StatTracker.GetEffectiveMaxValue(StatType.Health);
            float currentHealth = IslandSurvivor.Globals.ServiceRegistry.Instance.StatTracker.GetCurrentValue(StatType.Health);
            UpdateHealthUI(currentHealth, maxHealth);
        }

        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.Connect(SignalManager.SignalName.StatChanged, Callable.From<int, float, float>(OnStatChanged));
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
        if (p_disposing && SignalManager.Instance != null && !Engine.IsEditorHint())
        {
            SignalManager.Instance.Disconnect(SignalManager.SignalName.StatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
        base.Dispose(p_disposing);
    }
}
