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
    private IEventBus m_eventBus;

    [Export]
    private bool m_isGlobal;

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

        if (m_isGlobal)
        {
            m_eventBus = Globals.ServiceRegistry.Instance.EventBus;
            m_statTracker = Globals.ServiceRegistry.Instance.StatTracker;
            m_eventBus.Subscribe<ProfileLoadedEvent>(OnProfileLoaded);
        }
        else
        {
            m_eventBus = new EventBus();
            m_statTracker = new StatTracker(m_eventBus);
        }

        m_eventBus.Subscribe<StatChangedEvent>(OnStatChangedEvent);

        if (m_baseStatsResource is PlayerStats)
        {
            SignalManager.Instance.StatUpgradePurchased += OnStatUpgradePurchased;
        }

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
        if (!m_isGlobal)
        {
            m_eventBus?.ProcessEvents();
        }
    }

    private void OnProfileLoaded(ProfileLoadedEvent e)
    {
        if (e.Profile?.Stats != null)
        {
            GD.Print("[StatManager] Profile Loaded. Syncing global stats.");
            SetCurrentValue(StatType.Health, e.Profile.Stats.Health);
            SetCurrentValue(StatType.Attack, e.Profile.Stats.Attack);
            SetCurrentValue(StatType.Speed, e.Profile.Stats.Speed);
            SetCurrentValue(StatType.Luck, e.Profile.Stats.Luck);
        }
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
            if (m_eventBus != null)
            {
                m_eventBus.Unsubscribe<StatChangedEvent>(OnStatChangedEvent);
                if (m_isGlobal)
                {
                    m_eventBus.Unsubscribe<ProfileLoadedEvent>(OnProfileLoaded);
                }
            }
            if (SignalManager.Instance != null)
            {
                SignalManager.Instance.StatUpgradePurchased -= OnStatUpgradePurchased;
            }
        }
        base.Dispose(p_disposing);
    }
}
