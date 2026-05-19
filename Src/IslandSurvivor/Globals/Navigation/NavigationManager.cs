using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Core.Domain;
using Core.Domain.Models;
using Core.Interfaces.Stats;
using Core.Interfaces;

namespace IslandSurvivor.Globals.Navigation;

public partial class NavigationManager : Node
{
    public static NavigationManager Instance { get; private set; } = null!;

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
        SignalManager.Instance.TeleportRequested += OnTeleportRequested;
        GD.Print("NavigationManager ready. Listening for teleport requests.");
    }

    private async void OnTeleportRequested(string p_islandId, string p_scenePath, string p_biome, int p_difficulty, int p_resourceCost, int p_dangerLevel)
    {
        GD.Print($"[Navigation] Changing scene to {p_scenePath} (Island ID: {p_islandId})");

        // Use SceneLoadingManager if available
        Managers.SceneLoadingManager slm = GetNodeOrNull<Managers.SceneLoadingManager>("/root/SceneLoadingManager");
        if (slm != null)
        {
            slm.ShowLoading();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        // Sync Data using ApiService
        if (ServiceRegistry.Instance?.ApiService != null)
        {
            SyncRequest syncRequest = new SyncRequest();

            // Build Inventory Sync
            if (InventoryNode.Instance != null && InventoryNode.Instance.Manager != null)
            {
                IReadOnlyList<InventorySlot> slots = InventoryNode.Instance.Manager.GetAllSlots();
                List<InventoryEntry> inventoryEntries = new List<InventoryEntry>();
                foreach (InventorySlot slot in slots)
                {
                    inventoryEntries.Add(new InventoryEntry
                    {
                        ResourceItemId = slot.Item.Id,
                        Quantity = slot.Quantity
                    });
                }
                syncRequest.Inventory = inventoryEntries;
            }

            // Update session state locally before sync
            IScoreTracker tracker = ServiceRegistry.Instance.ScoreTracker;
            if (tracker != null)
            {
                tracker.UpdateCurrentIsland(p_islandId);
            }

            // Fire sync task
            bool success = await ServiceRegistry.Instance.ApiService.SyncAsync(syncRequest);
            if (success)
            {
                GD.Print("[Navigation] Sync to API complete.");
            }
            else
            {
                GD.Print("[Navigation] Sync to API failed, data cached locally.");
            }
        }

        // Perform transition
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
        Error error = GetTree().ChangeSceneToFile(p_scenePath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[Navigation] Failed to change scene to {p_scenePath}. Error: {error}");
        }
    }

    public override void _ExitTree()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.TeleportRequested -= OnTeleportRequested;
        }
    }
}
