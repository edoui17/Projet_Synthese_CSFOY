using Godot;
using System;
using Core.Domain.Models;
using Core.Interfaces;

namespace IslandSurvivor.Globals.Navigation;

public partial class NavigationManager : Node
{
    public static NavigationManager Instance { get; private set; }

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
        SignalManager.Instance.NavigationRequested += OnNavigationRequested;
        GD.Print("NavigationManager ready. Listening for navigation requests.");
    }

    private void OnNavigationRequested(string p_islandId, string p_scenePath, string p_biome, int p_difficulty, int p_resourceCost, int p_dangerLevel)
    {
        GD.Print($"[Navigation] Changing scene to {p_scenePath} (Island ID: {p_islandId})");

        // Save current inventory to persist between islands
        if (InventoryNode.Instance != null && InventoryNode.Instance.Manager != null)
        {
            var saveService = new GodotSaveService();
            var slots = InventoryNode.Instance.Manager.GetAllSlots();

            // Serialize and save inventory
            string inventoryJson = System.Text.Json.JsonSerializer.Serialize(slots);
            saveService.SaveData("inventory_save.json", inventoryJson);
            GD.Print("[Navigation] Inventory state saved.");

            // Save SessionState
        // ScoreTracker is now in ServiceRegistry
        var tracker = ServiceRegistry.Instance.ScoreTracker;
        if (tracker != null)
            {
            tracker.UpdateCurrentIsland(p_islandId);

                string sessionJson = System.Text.Json.JsonSerializer.Serialize(tracker.GetSessionState());
                saveService.SaveData("session_save.json", sessionJson);
                GD.Print("[Navigation] Session state saved.");
            }
            else
            {
            GD.PrintErr("[Navigation] Could not find ScoreTracker to save SessionState.");
            }
        }

        // Perform transition
        // Use SceneLoadingManager if available
        var slm = GetNodeOrNull<Managers.SceneLoadingManager>("/root/SceneLoadingManager");
        if (slm != null)
        {
        slm.LoadScene(p_scenePath);
        }
        else
        {
        CallDeferred(nameof(ChangeScene), p_scenePath);
        }
    }

    private void ChangeScene(string p_scenePath)
    {
        var error = GetTree().ChangeSceneToFile(p_scenePath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[Navigation] Failed to change scene to {p_scenePath}. Error: {error}");
        }
    }

    public override void _ExitTree()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.NavigationRequested -= OnNavigationRequested;
        }
    }
}
