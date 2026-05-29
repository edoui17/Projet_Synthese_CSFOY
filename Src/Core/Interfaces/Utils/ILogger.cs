namespace Core.Interfaces;

/// <summary>
/// Provides logging abstractions completely decoupled from the rendering engine.
/// </summary>
public interface ILogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
}
