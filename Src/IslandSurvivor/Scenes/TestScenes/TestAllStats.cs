using Godot;
using System;
using Core.Managers.Stats;
using IslandSurvivor.Globals;

public partial class TestAllStats : Control
{
    private Label _statsDisplayLabel;
    private float _health = 100f, _maxHealth = 100f;
    private float _attack = 10f;
    private float _speed = 300f;
    private float _luck = 1f;

    public override void _Ready()
    {
        _statsDisplayLabel = GetNode<Label>("VBoxContainer/StatsDisplay");

        Button btnHealth = GetNode<Button>("VBoxContainer/HBoxContainer/BtnHealth");
        Button btnAttack = GetNode<Button>("VBoxContainer/HBoxContainer/BtnAttack");
        Button btnSpeed = GetNode<Button>("VBoxContainer/HBoxContainer/BtnSpeed");
        Button btnLuck = GetNode<Button>("VBoxContainer/HBoxContainer/BtnLuck");
        Button btnDamage = GetNode<Button>("VBoxContainer/HBoxContainer2/BtnDamage");

        btnHealth.Pressed += () => UpgradeStat(StatType.Health);
        btnAttack.Pressed += () => UpgradeStat(StatType.Attack);
        btnSpeed.Pressed += () => UpgradeStat(StatType.Speed);
        btnLuck.Pressed += () => UpgradeStat(StatType.Luck);

        btnDamage.Pressed += TakeDamage;

        // Fetch initial values
        if (ServiceRegistry.Instance != null && ServiceRegistry.Instance.StatTracker != null)
        {
            _maxHealth = ServiceRegistry.Instance.StatTracker.GetEffectiveMaxValue(StatType.Health);
            _health = ServiceRegistry.Instance.StatTracker.GetCurrentValue(StatType.Health);
            _attack = ServiceRegistry.Instance.StatTracker.GetEffectiveMaxValue(StatType.Attack);
            _speed = ServiceRegistry.Instance.StatTracker.GetEffectiveMaxValue(StatType.Speed);
            _luck = ServiceRegistry.Instance.StatTracker.GetEffectiveMaxValue(StatType.Luck);
        }

        UpdateDisplay();

        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.Connect(SignalManager.SignalName.StatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
    }

    private void UpgradeStat(StatType p_type)
    {
        if (SignalManager.Instance != null)
        {
            GD.Print($"[TestAllStats] Triggering upgrade for {p_type}");
            SignalManager.Instance.EmitStatUpgradePurchased(this, p_type);
        }
    }

    private void TakeDamage()
    {
        if (ServiceRegistry.Instance != null && ServiceRegistry.Instance.StatTracker != null)
        {
            GD.Print($"[TestAllStats] Taking 15 Damage");
            ServiceRegistry.Instance.StatTracker.ModifyCurrentValue(StatType.Health, -15f);
        }
    }

    private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        StatType type = (StatType)p_statType;

        if (type == StatType.Health)
        {
            _health = p_currentValue;
            _maxHealth = p_effectiveMaxValue;
        }
        else if (type == StatType.Attack)
            _attack = p_effectiveMaxValue;
        else if (type == StatType.Speed)
            _speed = p_effectiveMaxValue;
        else if (type == StatType.Luck)
            _luck = p_effectiveMaxValue;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        _statsDisplayLabel.Text = $"=== PLAYER STATS ===\n\n" +
                                  $"Health: {_health} / {_maxHealth}\n" +
                                  $"Attack: {_attack}\n" +
                                  $"Speed: {_speed}\n" +
                                  $"Luck: {_luck}\n\n" +
                                  $"Check the HealthBar in the HUD!";
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
