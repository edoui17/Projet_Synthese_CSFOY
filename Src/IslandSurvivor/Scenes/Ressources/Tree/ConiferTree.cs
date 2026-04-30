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
  [Export] public StatManager Stats { get; set; }
  [Export] public ScoreManager Scorer { get; set; }
  [Export] public string EntityId { get; set; } = "tree_conifer_01";
  [Export] public Timer Timer { get; set; }

  [Export] public string MaterialName { get; set; } = "Conifère";
  [Export] public string MaterialType { get; set; } = "Wood";
  [Export] public string IconPath { get; set; } = "res://Assets/Tiny Swords/Tiny Swords (Update 010)/Resources/Trees/Tree.png";

  public override void _Ready()
  {
    if (Stats != null)
    {
      Stats.SetCurrentValue(StatType.Health, 2500);
    }
    AreaEntered += OnAreaEntered;
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
        DestroyResource();
      }
    }
  }

  public void DestroyResource()
  {
    Random random = new();
    int quantity = random.Next(1, 5);

    var item = new Core.Domain.ResourceItem(EntityId, MaterialName, MaterialType, IconPath);
    SignalManager.Instance.EmitMaterialDestroyed(this, item, quantity);
    QueueFree();
  }

  public void TakeDamage(int p_amount, object p_attacker)
  {
    if (Stats == null) return;

    Stats.ModifyCurrentValue(StatType.Health, -p_amount);

    IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);

    if (Stats.GetCurrentValue(StatType.Health) <= 0)
    {
      DestroyResource();
    }
  }
}
