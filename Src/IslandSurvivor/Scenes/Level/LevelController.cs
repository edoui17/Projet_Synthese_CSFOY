using Godot;
using IslandSurvivor.Resources;
using Core.Interfaces;
using IslandSurvivor.Globals;

namespace IslandSurvivor.Scenes.Level;

/// <summary>
/// [Gameplay][Map]
/// Controls the level lifecycle, acting as the Single Source of Truth for the active scene.
/// </summary>
public partial class LevelController : Node
{
    [Export] public IslandConfig Config { get; set; } = null!;

    public override void _Ready()
    {
        if (Config == null)
        {
            GD.PushWarning($"[LevelController] No IslandConfig assigned to {Name}!");
            return;
        }

        IDifficultyManager? diffManager = ServiceRegistry.Instance?.DifficultyManager;
        if (diffManager != null)
        {
            diffManager.ResetTime();
            diffManager.SetIslandDifficulty(Config.DifficultyLevel);
            GD.Print($"[LevelController] Applied Island Config. Difficulty: {Config.DifficultyLevel}");
        }
        else
        {
            GD.PushError("[LevelController] DifficultyManager not found in ServiceRegistry!");
        }
    }
}
