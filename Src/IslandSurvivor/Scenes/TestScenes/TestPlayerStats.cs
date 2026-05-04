using Godot;
using System;
using Core.Managers.Stats;

public partial class TestPlayerStats : Node2D
{
    private Player m_player;

    public override void _Ready()
    {
        m_player = GetNode<Player>("Player");

        Button btnDamage = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnDamagePlayer");
        Button btnHealth = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnUpgradeHealth");
        Button btnAttack = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnUpgradeAttack");
        Button btnSpeed = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnUpgradeSpeed");
        Button btnLuck = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnUpgradeLuck");

        btnDamage.Pressed += () => m_player?.TakeDamage(10, null);

        btnHealth.Pressed += () => UpgradeStat(StatType.Health);
        btnAttack.Pressed += () => UpgradeStat(StatType.Attack);
        btnSpeed.Pressed += () => UpgradeStat(StatType.Speed);
        btnLuck.Pressed += () => UpgradeStat(StatType.Luck);
    }

    private void UpgradeStat(StatType p_type)
    {
        if (SignalManager.Instance != null)
        {
            GD.Print($"[TestPlayerStats] Requesting upgrade for {p_type}");
            SignalManager.Instance.EmitStatUpgradePurchased(this, p_type);
        }
    }
}
