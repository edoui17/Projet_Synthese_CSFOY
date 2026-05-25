using Core.Interfaces;

namespace Core.Events;

public class BossDiedEvent : IEvent
{
    public string BossId { get; }
    public string BossType { get; }

    public BossDiedEvent(string p_bossId, string p_bossType)
    {
        BossId = p_bossId;
        BossType = p_bossType;
    }
}
