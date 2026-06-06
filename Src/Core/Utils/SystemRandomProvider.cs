using System;
using Core.Interfaces;

namespace Core.Utils;

/// <summary>
/// A standard implementation of IRandomProvider utilizing System.Random.
/// </summary>
public class SystemRandomProvider : IRandomProvider
{
    private readonly Random m_random;

    public SystemRandomProvider()
    {
        m_random = new Random();
    }

    public int Next(int maxValue)
    {
        return m_random.Next(maxValue);
    }

    public double NextDouble()
    {
        return m_random.NextDouble();
    }
}
