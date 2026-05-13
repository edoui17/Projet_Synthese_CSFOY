using Godot;
using System;

[Tool]
public partial class HealthBarLvl : Control
{
    private int _level = 0;

    [Export]
    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            UpdateSize();
        }
    }

    [Export] public TextureRect CenterBar = null!;

    [Export] public float BaseWidth = 40f;
    [Export] public float WidthPerLevel = 20f;

    [Export] public TextureProgressBar ProgressBar = null!;

    [Export] public float BaseHealth = 100f;
    [Export] public float HealthPerLevel = 25f;

    private float _currentHealth;

    private Control _container = null!;

    public override void _Ready()
    {
        _container = GetNode<Control>("HBoxContainer");
        UpdateSize();
    }

    private void UpdateSize()
    {
        if (CenterBar == null || ProgressBar == null)
        {
            return;
        }

        float width = BaseWidth + (Level * WidthPerLevel);
        CenterBar.CustomMinimumSize = new Vector2(width, CenterBar.CustomMinimumSize.Y);

        float maxHealth = BaseHealth + (Level * HealthPerLevel);
        ProgressBar.MaxValue = maxHealth;
        ProgressBar.Value = maxHealth;

        GD.Print(maxHealth);
    }

}
