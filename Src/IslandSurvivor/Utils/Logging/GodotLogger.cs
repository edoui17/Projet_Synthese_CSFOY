using Godot;
using Core.Interfaces;

namespace IslandSurvivor.Utils.Logging;

/// <summary>
/// A Godot-specific implementation of ILogger that forwards log messages to the Godot console.
/// </summary>
public class GodotLogger : ILogger
{
    public void LogInfo(string message)
    {
        GD.Print(message);
    }

    public void LogWarning(string message)
    {
        GD.PushWarning(message);
    }

    public void LogError(string message)
    {
        GD.PushError(message);
    }
}
