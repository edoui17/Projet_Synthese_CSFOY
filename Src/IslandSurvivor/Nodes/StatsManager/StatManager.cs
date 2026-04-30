using System.Collections.Generic;
using Godot;
using Core.Interfaces.Stats;
using Core.Interfaces;
using Core.Managers.Stats;
using Core.Services;
using Core.Events;
using IslandSurvivor.Resources;

namespace IslandSurvivor.Nodes;

public partial class StatManager : Node
{
    private EntityStats? m_baseStatsResource;
    private IStatTracker m_statTracker;
    private IEventBus m_localEventBus;

    [Signal]
    public delegate void LocalStatChangedEventHandler(int p_statType, float p_currentValue, float p_effectiveMaxValue);

    [Export]
    public EntityStats? BaseStatsResource
    {
        get => m_baseStatsResource;
        set => m_baseStatsResource = value;
    }

    public override void _Ready()
    {
        base._Ready();

        m_localEventBus = new EventBus();
        m_statTracker = new StatTracker(m_localEventBus);

        m_localEventBus.Subscribe<StatChangedEvent>(OnStatChangedEvent);

        SignalManager.Instance.StatUpgradePurchased += OnStatUpgradePurchased;

        if (m_baseStatsResource != null)
        {
            Dictionary<StatType, float> initialStats = new Dictionary<StatType, float>
            {
                { StatType.Health, m_baseStatsResource.MaxHealth }
            };

            if (m_baseStatsResource is CombatEntityStats combatStats)
            {
                initialStats.Add(StatType.Attack, combatStats.Attack);
                initialStats.Add(StatType.Speed, combatStats.Speed);
            }

            if (m_baseStatsResource is PlayerStats playerStats)
            {
                initialStats.Add(StatType.Luck, playerStats.Luck);
            }

            m_statTracker.InitializeStats(initialStats);
        }
        else
        {
            GD.PushWarning("StatManager: BaseStatsResource is not assigned.");
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        m_localEventBus?.ProcessEvents();
    }

    private void OnStatChangedEvent(StatChangedEvent e)
    {
        EmitSignal(SignalName.LocalStatChanged, (int)e.StatType, e.CurrentValue, e.EffectiveMaxValue);
    }

    public float GetCurrentValue(StatType p_statType)
    {
        return m_statTracker.GetCurrentValue(p_statType);
    }

    public float GetEffectiveMaxValue(StatType p_statType)
    {
        return m_statTracker.GetEffectiveMaxValue(p_statType);
    }

    public void ModifyCurrentValue(StatType p_statType, float p_amount)
    {
        m_statTracker.ModifyCurrentValue(p_statType, p_amount);
    }

    public void SetCurrentValue(StatType p_statType, float p_value)
    {
        m_statTracker.SetCurrentValue(p_statType, p_value);
    }

    public void AddPermanentBonus(StatType p_statType, float p_amount)
    {
        m_statTracker.AddPermanentBonus(p_statType, p_amount);
    }

    private void OnStatUpgradePurchased(int p_statType)
    {
        StatType type = (StatType)p_statType;
        float amount = type == StatType.Health ? 10f : 1f;

        AddPermanentBonus(type, amount);
        GD.Print($"[StatManager] Received StatUpgradePurchased for {type}. Adding +{amount} permanent bonus.");
    }

    protected override void Dispose(bool p_disposing)
    {
        if (p_disposing)
        {
            if (m_localEventBus != null)
            {
                m_localEventBus.Unsubscribe<StatChangedEvent>(OnStatChangedEvent);
            }
            if (SignalManager.Instance != null)
            {
                SignalManager.Instance.StatUpgradePurchased -= OnStatUpgradePurchased;
            }
        }
        base.Dispose(p_disposing);
    }
}
