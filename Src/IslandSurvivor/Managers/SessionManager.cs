using Godot;
using IslandSurvivor.Globals;
using Core.Interfaces;
using Core.Interfaces;

namespace IslandSurvivor.Managers;

public partial class SessionManager : Node
{
    public static SessionManager Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public override void _Ready()
    {
        SignalManager.Instance.SessionEnded += OnSessionEnded;
    }

    private void OnSessionEnded(bool p_isVictory)
    {
        GD.Print($"[SessionManager] Session Ended signal received. Victory: {p_isVictory}. Cleaning up session state...");

        // 1. Clear Inventory
        IInventoryManager inventoryManager = ServiceRegistry.Instance.InventoryManager;
        inventoryManager?.ClearInventory();

        // 2. Reset Session & Score
        IScoreTracker scoreTracker = ServiceRegistry.Instance.ScoreTracker;
        scoreTracker?.ResetSession();

        // 3. Reset Stats
        IStatTracker statTracker = ServiceRegistry.Instance.StatTracker;
        statTracker?.ResetStats();

        // 4. Delete Temporary Profile Cache (user://profile_cache.json)
        ISaveService saveService = ServiceRegistry.Instance.SaveService;
        saveService?.DeleteData("profile_cache.json");

        GD.Print("[SessionManager] Session state successfully cleaned. Ready for Session 0.");
    }

    protected override void Dispose(bool p_disposing)
    {
        if (p_disposing && SignalManager.Instance != null)
        {
            SignalManager.Instance.SessionEnded -= OnSessionEnded;
        }
        base.Dispose(p_disposing);
    }
}
