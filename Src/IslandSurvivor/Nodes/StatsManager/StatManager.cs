using System.Collections.Generic;
using Godot;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using IslandSurvivor.Resources;

namespace IslandSurvivor.Nodes;

public partial class StatManager : Node
{
    private EntityStats? m_baseStatsResource;
    private IStatTracker m_statTracker;

    [Export]
    public EntityStats? BaseStatsResource
    {
        get => m_baseStatsResource;
        set => m_baseStatsResource = value;
    }

    [Signal]
    public delegate void StatChangedEventHandler(int p_statType, float p_currentValue, float p_effectiveMaxValue);

    public StatManager()
    {
        m_statTracker = new StatTracker();
    }

    public override void _Ready()
    {
        base._Ready();

        if (m_baseStatsResource != null)
        {
            Dictionary<StatType, float> initialStats = new Dictionary<StatType, float>
            {
                { StatType.Health, m_baseStatsResource.MaxHealth },
                { StatType.Attack, m_baseStatsResource.Attack },
                { StatType.Speed, m_baseStatsResource.Speed },
                { StatType.Luck, m_baseStatsResource.Luck }
            };

            m_statTracker.InitializeStats(initialStats);
        }
        else
        {
            GD.PushWarning("StatManager: BaseStatsResource is not assigned.");
        }

        m_statTracker.OnAnyStatChanged.AddListener(OnCoreStatChanged);
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

    private void OnCoreStatChanged(object? p_sender, StatChangedEventArgs p_args)
    {
        EmitSignal(SignalName.StatChanged, (int)p_args.StatType, p_args.CurrentValue, p_args.EffectiveMaxValue);
    }

    protected override void Dispose(bool p_disposing)
    {
        if (p_disposing && m_statTracker != null)
        {
            m_statTracker.OnAnyStatChanged.RemoveListener(OnCoreStatChanged);
        }
        base.Dispose(p_disposing);
    }
}
