using Godot;

namespace IslandSurvivor.Resources.Stats;

[GlobalClass]
public partial class SessionResource : Resource
{
    private int m_startingScore;
    private int m_startingMapCount;
    private string m_startingCharacterId = string.Empty;

    [Export]
    public int StartingScore
    {
        get => m_startingScore;
        set => m_startingScore = value;
    }

    [Export]
    public int StartingMapCount
    {
        get => m_startingMapCount;
        set => m_startingMapCount = value;
    }

    [Export]
    public string StartingCharacterId
    {
        get => m_startingCharacterId;
        set => m_startingCharacterId = value;
    }
}
