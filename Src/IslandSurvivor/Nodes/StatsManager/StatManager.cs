using System.Collections.Generic;
using Godot;
using Core.Interfaces;
using Core.Interfaces;
using Core.Managers;
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

    [ExportGroup("Base Values")]
    [Export] public float MaxHealth { get; set; } = 100f;
    [Export] public float BaseAttackValue { get; set; } = 10f;
    [Export] public float BaseSpeedValue { get; set; } = 300f;

    [ExportGroup("Initial Stat Points")]
    [Export] public float InitialAttackPoints { get; set; } = 0f;
    [Export] public float InitialSpeedPoints { get; set; } = 0f;
    [Export] public float InitialLuckPoints { get; set; } = 0f;

    [Signal]
    public delegate void LocalStatChangedEventHandler(int p_statType, float p_currentValue, float p_effectiveMaxValue);

    public override void _ValidateProperty(Godot.Collections.Dictionary p_property)
    {
        if (!Engine.IsEditorHint()) return;

        string name = p_property["name"].AsString();

        if (m_entityType == EntityType.Resource && (name == "BaseAttackValue" || name == "BaseSpeedValue" || name == "InitialAttackPoints" || name == "InitialSpeedPoints" || name == "InitialLuckPoints"))
        {
            var usage = p_property["usage"].As<PropertyUsageFlags>();
            p_property["usage"] = (int)(usage & ~PropertyUsageFlags.Editor);
            return;
        }

        if (m_entityType == EntityType.NPC && name == "InitialLuckPoints")
        {
            var usage = p_property["usage"].As<PropertyUsageFlags>();
            p_property["usage"] = (int)(usage & ~PropertyUsageFlags.Editor);
            return;
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
            initialStats.Add(StatType.Attack, InitialAttackPoints);
            initialStats.Add(StatType.Speed, InitialSpeedPoints);
        }

        if (m_entityType == EntityType.Player)
        {
            initialStats.Add(StatType.Luck, InitialLuckPoints);
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

    private void OnProfileLoaded(ProfileLoadedEvent p_event)
    {
        var bestStats = p_event.Profile?.GameStats?.OrderByDescending(s => s.Score).FirstOrDefault();
        if (bestStats != null)
        {
            GD.Print("[StatManager] Profile Loaded. Syncing global stats from best session.");
            SetCurrentValue(StatType.Health, bestStats.Health);
            SetCurrentValue(StatType.Attack, bestStats.Attack);
            SetCurrentValue(StatType.Speed, bestStats.Speed);
            SetCurrentValue(StatType.Luck, bestStats.Luck);
        }
    }

    private void OnStatChangedEvent(StatChangedEvent p_event)
    {
        EmitSignal(SignalName.LocalStatChanged, (int)p_event.StatType, p_event.CurrentValue, p_event.EffectiveMaxValue);
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

    public void AddSessionBonus(StatType p_statType, float p_amount)
    {
        m_statTracker.AddSessionBonus(p_statType, p_amount);
    }

    private void OnStatUpgradePurchased(int p_statType)
    {
        StatType type = (StatType)p_statType;
        float amount = type == StatType.Health ? 10f : 1f;

        AddSessionBonus(type, amount);
        GD.Print($"[StatManager] Received StatUpgradePurchased for {type}. Adding +{amount} permanent bonus.");
    }

    protected override void Dispose(bool p_disposing)
    {
        if (!p_disposing)
        {
            base.Dispose(p_disposing);
            return;
        }

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

        base.Dispose(p_disposing);
    }
}
