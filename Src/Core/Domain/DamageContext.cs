namespace Core.Domain;

public class DamageContext
{
    public float AttackerLuck { get; }
    public object? Attacker { get; }

    public DamageContext(float p_attackerLuck, object? p_attacker = null)
    {
        AttackerLuck = p_attackerLuck;
        Attacker = p_attacker;
    }
}
