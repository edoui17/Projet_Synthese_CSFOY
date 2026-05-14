using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes;
using IslandSurvivor.Nodes.StatsManager;
using IslandSurvivor.Resources;
using System;

public partial class ConiferTree : Area2D, ITree, IDamageable
{
  [Export] public StatManager Stats { get; set; } = null!;
  [Export] public ScoreManager Scorer { get; set; } = null!;
  [Export] public string EntityId { get; set; } = "tree_conifer_01";
  [Export] public Timer Timer { get; set; } = null!;

  [Export] public string MaterialName { get; set; } = "Conifère";
  [Export] public string MaterialType { get; set; } = "Wood";
  [Export] public string IconPath { get; set; } = "res://Assets/Tiny Swords/Tiny Swords (Update 010)/Resources/Trees/Tree.png";

  private object? m_lastAttacker;

  public override void _Ready()
  {
    if (Stats != null)
    {
      Stats.SetCurrentValue(StatType.Health, 40);
      Stats.Connect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
    }
    AreaEntered += OnAreaEntered;
  }

  private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
  {
      if ((StatType)p_statType == StatType.Health && p_currentValue <= 0)
      {
          if (Stats != null)
          {
              Stats.Disconnect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
          }
          DestroyResource(m_lastAttacker);
      }
  }

  public void OnAreaEntered(Area2D p_area)
  {
    if (p_area.IsInGroup("Tool"))
    {
      if (Timer == null || Timer.IsStopped())
      {
        Timer?.Start();
        Scorer.AddScore(1);
        Scorer.UpdateHighScore();
        DestroyResource(p_area.GetParent() ?? p_area);
      }
    }
  }

  public void DestroyResource(object? p_attacker = null)
  {
    Random random = new();
    int quantity = random.Next(1, 5);

    float luck = 0f;
    if (p_attacker is Node GodotAttacker)
    {
        StatManager attackerStats = GodotAttacker.GetNodeOrNull<StatManager>("StatManager");
        if (attackerStats != null)
        {
            luck = attackerStats.GetCurrentValue(StatType.Luck);
        }
    }

    float bonusChance = luck * 0.05f;
    int bonusQuantity = (int)bonusChance;
    float fractionalChance = bonusChance - bonusQuantity;

    if (random.NextDouble() < fractionalChance)
    {
        bonusQuantity++;
    }

    quantity += bonusQuantity;

    var item = new Core.Domain.ResourceItem(EntityId, MaterialName, MaterialType, IconPath);
    SignalManager.Instance.EmitMaterialDestroyed(this, item, quantity);
    QueueFree();
  }

  public void TakeDamage(int p_amount, object p_attacker)
  {
    if (Stats == null) return;

    m_lastAttacker = p_attacker;
    Stats.ModifyCurrentValue(StatType.Health, -p_amount);

    IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);
  }
}
