using System.Collections.Generic;
using Godot;
using Core.Interfaces.Stats;
using Core.Interfaces;
using Core.Managers.Stats;
using Core.Services;
using Core.Events;
using IslandSurvivor.Resources;
using IslandSurvivor.Enums;
using System.Linq;

namespace IslandSurvivor.Nodes;

[Tool]
public partial class StatManager : Node2D
{
    private IStatTracker m_statTracker = null!;
    private IEventBus m_eventBus = null!;

    [Export]
    private bool m_isGlobal;

    private EntityType m_entityType = EntityType.NPC;

    [Export]
    public EntityType EntityType
    {
        get => m_entityType;
        set
        {
            m_entityType = value;
            NotifyPropertyListChanged();
        }
    }

    [ExportGroup("Base Stats")]
    [Export] public float MaxHealth { get; set; } = 100f;
    [Export] public float BaseDamage { get; set; } = 10f;
    [Export] public float BaseSpeed { get; set; } = 300f;
    [Export] public float Luck { get; set; } = 0f;

    [Signal]
    public delegate void LocalStatChangedEventHandler(int p_statType, float p_currentValue, float p_effectiveMaxValue);

    public override void _ValidateProperty(Godot.Collections.Dictionary property)
    {
        if (!Engine.IsEditorHint()) return;

        string name = property["name"].AsString();

        if (m_entityType == EntityType.Resource)
        {
            if (name == "BaseDamage" || name == "BaseSpeed" || name == "Luck")
            {
                var usage = property["usage"].As<PropertyUsageFlags>();
                property["usage"] = (int)(usage & ~PropertyUsageFlags.Editor);
            }
        }
        else if (m_entityType == EntityType.NPC)
        {
            if (name == "Luck")
            {
                var usage = property["usage"].As<PropertyUsageFlags>();
                property["usage"] = (int)(usage & ~PropertyUsageFlags.Editor);
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) return;

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

        if (m_isGlobal)
        {
            SignalManager.Instance.StatUpgradePurchased += OnStatUpgradePurchased;
        }

        Dictionary<StatType, float> initialStats = new Dictionary<StatType, float>
        {
            { StatType.Health, MaxHealth }
        };

        if (m_entityType == EntityType.NPC || m_entityType == EntityType.Player)
        {
            initialStats.Add(StatType.Attack, BaseDamage);
            initialStats.Add(StatType.Speed, BaseSpeed);
        }

        if (m_entityType == EntityType.Player)
        {
            initialStats.Add(StatType.Luck, Luck);
        }

        if (m_isGlobal && m_statTracker.IsInitialized)
        {
            GD.Print("[StatManager] Global stats already initialized, skipping reset.");
        }
        else
        {
            m_statTracker.InitializeStats(initialStats);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Engine.IsEditorHint()) return;

        if (!m_isGlobal)
        {
            m_eventBus?.ProcessEvents();
        }
    }

    private void OnProfileLoaded(ProfileLoadedEvent e)
    {
        var bestStats = e.Profile?.GameStats?.OrderByDescending(s => s.Score).FirstOrDefault();
        if (bestStats != null)
        {
            GD.Print("[StatManager] Profile Loaded. Syncing global stats from best session.");
            SetCurrentValue(StatType.Health, bestStats.Health);
            SetCurrentValue(StatType.Attack, bestStats.Attack);
            SetCurrentValue(StatType.Speed, bestStats.Speed);
            SetCurrentValue(StatType.Luck, bestStats.Luck);
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
            if (m_eventBus != null && !Engine.IsEditorHint())
            {
                m_eventBus.Unsubscribe<StatChangedEvent>(OnStatChangedEvent);
                if (m_isGlobal)
                {
                    m_eventBus.Unsubscribe<ProfileLoadedEvent>(OnProfileLoaded);
                }
            }
            if (SignalManager.Instance != null && !Engine.IsEditorHint())
            {
                SignalManager.Instance.StatUpgradePurchased -= OnStatUpgradePurchased;
            }
        }
        base.Dispose(p_disposing);
    }
}
