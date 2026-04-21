namespace Core.Managers.Stats;

using System.Text.Json;
using Core.Domain;
using Core.Interfaces;
using Core.Interfaces.Stats;
using Core.Utils;

public class ScoreTracker : IScoreTracker
{
    private readonly ISaveService m_saveService;
    private SessionState m_sessionState;
    private int m_highScore;
    private const string HIGH_SCORE_FILE = "highscore.json";

    private readonly WeakEvent<ScoreChangedEventArgs> m_onScoreChanged = new WeakEvent<ScoreChangedEventArgs>();
    public WeakEvent<ScoreChangedEventArgs> OnScoreChanged => m_onScoreChanged;

    public int CurrentScore => m_sessionState.Score;
    public int HighScore => m_highScore;
    public int MapCount => m_sessionState.MapCount;
    public string CharacterId => m_sessionState.CharacterId;
    public string CurrentIslandId => m_sessionState.CurrentIslandId;

    public SessionState GetSessionState() => m_sessionState;

    public void UpdateCurrentIsland(string p_islandId)
    {
        m_sessionState.CurrentIslandId = p_islandId;
    }

    public ScoreTracker(ISaveService p_saveService)
    {
        m_saveService = p_saveService;
        m_sessionState = new SessionState();
        LoadHighScore();
    }

    public void Initialize(int p_initialScore, int p_mapCount, string p_characterId)
    {
        m_sessionState = new SessionState(p_initialScore, p_mapCount, p_characterId);
    }

    public void AddScore(int p_amount)
    {
        if (p_amount <= 0)
        {
            return;
        }

        int previousScore = m_sessionState.Score;
        m_sessionState.Score += p_amount;

        m_onScoreChanged.Invoke(this, new ScoreChangedEventArgs(previousScore, m_sessionState.Score));
    }

    public void UpdateHighScore()
    {
        if (m_sessionState.Score > m_highScore)
        {
            m_highScore = m_sessionState.Score;
            SaveHighScore();
        }
    }

    private void LoadHighScore()
    {
        string jsonData = m_saveService.LoadData(HIGH_SCORE_FILE);
        if (!string.IsNullOrEmpty(jsonData))
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonData);
                if (doc.RootElement.TryGetProperty("HighScore", out JsonElement highScoreElement))
                {
                    m_highScore = highScoreElement.GetInt32();
                }
            }
            catch
            {
                m_highScore = 0;
            }
        }
    }

    private void SaveHighScore()
    {
        string jsonData = $"{{\"HighScore\":{m_highScore}}}";
        m_saveService.SaveData(HIGH_SCORE_FILE, jsonData);
    }
}
