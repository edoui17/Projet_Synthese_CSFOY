using Godot;
using System;
using IslandSurvivor.Globals;
using IslandSurvivor.Globals.Navigation;

namespace IslandSurvivor.Managers;

public partial class SceneLoadingManager : Node
{
    public static SceneLoadingManager Instance { get; private set; }

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public void LoadScene(string scenePath)
    {
        CallDeferred(nameof(ChangeScene), scenePath);
    }

    private void ChangeScene(string scenePath)
    {
        var error = GetTree().ChangeSceneToFile(scenePath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[SceneLoadingManager] Failed to load scene {scenePath}. Error: {error}");
        }
    }
}
