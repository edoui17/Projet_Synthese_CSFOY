namespace Core.Interfaces.Utils;

/// <summary>
/// Provides deterministic random number generation abstractions.
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    int Next(int maxValue);

    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
    /// </summary>
    double NextDouble();
}
