using Core.Domain;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Classes;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using System;

public partial class Rock : Area2D, IOre, IDamageable
{
    [Export] private IslandSurvivor.Nodes.StatManager _stats;


    [Export] public string EntityId { get; set; } = "rock_01";
    [Export] public Timer Timer { get; set; }

    [Export] public string MaterialName { get; set; } = "Roche";
    [Export] public string MaterialType { get; set; } = "Rock";
    [Export] public string IconPath { get; set; } = "res://Assets/TinySwords/TinySwords(Update010)/Deco/06.png";

    private DamageContext? m_lastContext;


    public float GetHealth()
    {
        return _stats?.GetCurrentValue(Core.Managers.Stats.StatType.Health) ?? 0f;
    }

    public void Heal(float p_amount)
    {
        _stats?.ModifyCurrentValue(Core.Managers.Stats.StatType.Health, p_amount);
    }

    public override void _Ready()
    {
        if (_stats != null)
        {
            _stats.SetCurrentValue(StatType.Health, 20);
            _stats.Connect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
        AreaEntered += OnAreaEntered;
    }

    private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        if ((StatType)p_statType == StatType.Health && p_currentValue <= 0)
        {
            if (_stats != null)
            {
                _stats.Disconnect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
            }
            DestroyResource(m_lastContext);
        }
    }

    private void OnAreaEntered(Area2D p_area)
    {
        if (p_area.IsInGroup("Tool"))
        {
            if (Timer == null || Timer.IsStopped())
            {
                Timer?.Start();
                // If it's a Godot Node, we can pass it as the attacker
                float attackerLuck = 0f;
                var attacker = p_area.GetParent() ?? p_area;
                if (attacker is ILuckyEntity luckyEntity)
                {
                    attackerLuck = luckyEntity.GetLuck();
                }
                var ctx = new DamageContext(attackerLuck, attacker);
                DestroyResource(ctx);
            }
        }
    }

    public void DestroyResource(DamageContext p_context = null)
    {
        Random random = new();
        int quantity = random.Next(1, 5);

        float luck = p_context?.AttackerLuck ?? 0f;

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

    void IGatheringMaterials.OnAreaEntered(Area2D p_area)
    {
        OnAreaEntered(p_area);
    }

    public void TakeDamage(int p_amount, DamageContext p_context)
    {
        if (_stats == null) return;

        m_lastContext = p_context;
        _stats.ModifyCurrentValue(StatType.Health, -p_amount);

        IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);
    }
}
