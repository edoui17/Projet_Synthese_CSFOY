using Core.Interfaces;

namespace Core.Events;

public class EnemyKilledEvent : IEvent
{
    public string EnemyId { get; }
    public string EnemyType { get; }

    public EnemyKilledEvent(string p_enemyId, string p_enemyType)
    {
        EnemyId = p_enemyId;
        EnemyType = p_enemyType;
    }
}
