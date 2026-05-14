using Core.Interfaces;

namespace Core.Events;

public class EnemyKilledEvent : IEvent
{
    public string EnemyId { get; }
    public string EnemyType { get; }
    public float XpEarned { get; }

    public EnemyKilledEvent(string p_enemyId, string p_enemyType, float p_xpEarned)
    {
        EnemyId = p_enemyId;
        EnemyType = p_enemyType;
        XpEarned = p_xpEarned;
    }
}
