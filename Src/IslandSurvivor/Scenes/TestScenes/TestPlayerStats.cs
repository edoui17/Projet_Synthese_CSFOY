using Godot;
using System;
using Core.Managers.Stats;

public partial class TestPlayerStats : Node2D
{
    private Player m_player = null!;

    public override void _Ready()
    {
        m_player = GetNode<Player>("Player");

        Button btnDamage = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnDamagePlayer");
        Button btnHealth = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnUpgradeHealth");
        Button btnAttack = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnUpgradeAttack");
        Button btnSpeed = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnUpgradeSpeed");
        Button btnLuck = GetNode<Button>("CanvasLayer/UIControls/VBoxContainer/BtnUpgradeLuck");

        btnDamage.Pressed += () => m_player?.TakeDamage(10, this);

        btnHealth.Pressed += () => UpgradeStat(StatType.Health);
        btnAttack.Pressed += () => UpgradeStat(StatType.Attack);
        btnSpeed.Pressed += () => UpgradeStat(StatType.Speed);
        btnLuck.Pressed += () => UpgradeStat(StatType.Luck);
    }

    private async void UpgradeStat(StatType p_type)
    {
        if (SignalManager.Instance != null)
        {
            float beforeVal = m_player?.Stats?.GetCurrentValue(p_type) ?? 0f;
            float beforeMaxVal = m_player?.Stats?.GetEffectiveMaxValue(p_type) ?? 0f;
            GD.Print($"[TestPlayerStats] BEFORE - {p_type}: Current={beforeVal}, Max={beforeMaxVal}");

            GD.Print($"[TestPlayerStats] Requesting upgrade for {p_type}");
            SignalManager.Instance.EmitStatUpgradePurchased(this, p_type);

            // Wait a frame so EventBus can process the queue and apply the upgrade
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            float afterVal = m_player?.Stats?.GetCurrentValue(p_type) ?? 0f;
            float afterMaxVal = m_player?.Stats?.GetEffectiveMaxValue(p_type) ?? 0f;
            GD.Print($"[TestPlayerStats] AFTER  - {p_type}: Current={afterVal}, Max={afterMaxVal}");
        }
    }
}
