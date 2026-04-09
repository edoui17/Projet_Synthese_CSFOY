namespace Core.Domain;

public class SessionState
{
    private int m_score;
    private int m_mapCount;
    private string m_characterId;
    private string m_currentIslandId;

    public int Score
    {
        get => m_score;
        set => m_score = value;
    }

    public int MapCount
    {
        get => m_mapCount;
        set => m_mapCount = value;
    }

    public string CharacterId
    {
        get => m_characterId;
        set => m_characterId = value;
    }

    public string CurrentIslandId
    {
        get => m_currentIslandId;
        set => m_currentIslandId = value;
    }

    public SessionState(int p_initialScore, int p_initialMapCount, string p_initialCharacterId, string p_initialIslandId = "home_hub")
    {
        m_score = p_initialScore;
        m_mapCount = p_initialMapCount;
        m_characterId = p_initialCharacterId;
        m_currentIslandId = p_initialIslandId;
    }

    public SessionState()
    {
        m_score = 0;
        m_mapCount = 0;
        m_characterId = string.Empty;
        m_currentIslandId = "home_hub";
    }
}
