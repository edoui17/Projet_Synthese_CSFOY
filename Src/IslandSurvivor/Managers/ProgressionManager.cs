using Godot;
using Core.Interfaces;
using Core.Events;
using IslandSurvivor.Resources;

namespace IslandSurvivor.Managers;

public partial class ProgressionManager : Node
{
    public static ProgressionManager Instance { get; private set; }

    [Export]
    public ProgressionRequirement Requirement { get; set; }

    // Configuration for XP rewards
    [ExportGroup("XP Rewards")]
    [Export] public float XpFromHarvesting = 10f;
    [Export] public float XpFromEnemyKill = 30f;
    [Export] public float XpFromNewIsland = 50f;

    private IEventBus m_eventBus;

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
            // Removed CallDeferred, since modifying tree in EnterTree outside ready is usually fine for Autoloads.
            // Keeping it simple.
        }
        else
        {
            QueueFree();
            return;
        }
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        m_eventBus = Globals.ServiceRegistry.Instance.EventBus;

        // Subscribe to relevant events
        m_eventBus.Subscribe<ResourceHarvestedEvent>(OnResourceHarvested);
        m_eventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        m_eventBus.Subscribe<NavigationRequestedEvent>(OnNavigationRequested);

        if (Requirement == null)
        {
            Requirement = new ProgressionRequirement();
        }
    }

    private void OnResourceHarvested(ResourceHarvestedEvent p_event)
    {
        // Reward XP for harvesting
        Globals.ServiceRegistry.Instance.StatTracker.AddExperience(XpFromHarvesting);
    }

    private void OnEnemyKilled(EnemyKilledEvent p_event)
    {
        // Reward more XP for killing enemies
        Globals.ServiceRegistry.Instance.StatTracker.AddExperience(XpFromEnemyKill);
    }

    private void OnNavigationRequested(NavigationRequestedEvent p_event)
    {
        // Don't reward XP for returning home, only for exploring
        if (p_event.Destination.Id != Core.Domain.Models.IslandDestination.HomeIsland.Id)
        {
            Globals.ServiceRegistry.Instance.StatTracker.AddExperience(XpFromNewIsland);
        }
    }

    public bool IsBossReady()
    {
        if (Requirement == null) return false;

        float currentLevel = Globals.ServiceRegistry.Instance.StatTracker.GetCurrentValue(Core.Managers.Stats.StatType.Level);
        return currentLevel >= Requirement.BossLevelRequirement;
    }
}
