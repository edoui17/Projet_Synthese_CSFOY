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

    public void LoadScene(string p_scenePath)
    {
        CallDeferred(nameof(ChangeScene), p_scenePath);
    }

    private void ChangeScene(string p_scenePath)
    {
        var error = GetTree().ChangeSceneToFile(p_scenePath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[SceneLoadingManager] Failed to load scene {p_scenePath}. Error: {error}");
        }
    }
}
