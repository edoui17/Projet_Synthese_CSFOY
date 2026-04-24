using Godot;
using System;

[Tool]
public partial class HealthBarStatic : Control
{
    private int _level = 0;

    [Export]
    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            UpdateHealth();
        }
    }

    [Export] public TextureProgressBar ProgressBar;
    [Export] public Label HealthLabel;

    [Export] public float BaseHealth = 100f;
    [Export] public float HealthPerLevel = 25f;

    private float _currentHealth = 100f;

    public override void _Ready()
    {
        UpdateHealth();
    }

    private void UpdateHealth()
    {
        if (ProgressBar == null)
        {
            return;
        }

        float maxHealth = BaseHealth + (Level * HealthPerLevel);

        ProgressBar.MaxValue = maxHealth;

        _currentHealth = maxHealth;

        ProgressBar.Value = _currentHealth;

        if (HealthLabel != null)
        {
            HealthLabel.Text = $"{(int)_currentHealth} / {(int)maxHealth}";
        }

        GD.Print(maxHealth);
    }

}
