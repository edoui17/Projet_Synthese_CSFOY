using Core.Interfaces;

namespace Core.Events;

public class ExperienceGainedEvent : IEvent
{
    public float Amount { get; }

    public ExperienceGainedEvent(float p_amount)
    {
        Amount = p_amount;
    }
}
