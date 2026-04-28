using Godot;
using System;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using Core.Managers.Stats; // Namespace for StatType

public partial class TestStats : Node2D
{
  // Point this to the "ConiferTree" node in the Inspector
  [Export]
  public Node2D TreeEntity;

  private StatManager _statManager;

  public override void _Ready()
  {
    if (TreeEntity == null)
    {
      GD.PrintErr("TestStats: Please assign the 'ConiferTree' node to the TreeEntity slot in the Inspector.");
      return;
    }

    // Based on your screenshot, the child node is named "StatsManager"
    _statManager = TreeEntity.GetNode<StatManager>("StatsManager");

    if (_statManager == null)
    {
      GD.PrintErr($"TestStats: Could not find a 'StatsManager' child node on {TreeEntity.Name}.");
      return;
    }

    // Connect the signal directly from the global SignalManager
    SignalManager.Instance.Connect(SignalManager.SignalName.StatChanged, Callable.From<int, float, float>(OnStatChanged));

    GD.Print($"--- Testing Stats for: {TreeEntity.Name} ---");

    // Run tests after a short delay to ensure initialization is complete
    GetTree().CreateTimer(0.5f).Timeout += RunTestSequence;
  }

  private void RunTestSequence()
  {
    // 1. Check initial Health
    float hp = _statManager.GetCurrentValue(StatType.Health);
    float maxHp = _statManager.GetEffectiveMaxValue(StatType.Health);
    GD.Print($"[TEST] Initial Health: {hp}/{maxHp}");

    // 2. Test Modification (Apply Damage)
    GD.Print("[TEST] Applying -10 Damage...");
    _statManager.ModifyCurrentValue(StatType.Health, -10f);

    // 3. Test Permanent Bonus (Buff Speed)
    GD.Print("[TEST] Adding +5 Permanent Speed Bonus...");
    _statManager.AddPermanentBonus(StatType.Speed, 5f);
  }

  private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
  {
    StatType type = (StatType)p_statType;
    GD.Print($"[SIGNAL] {type} Updated -> Current: {p_currentValue}, Max: {p_effectiveMaxValue}");
  }
}