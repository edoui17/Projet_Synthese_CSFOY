namespace Core.Interfaces.Stats;

public interface IDamageable
{
    void TakeDamage(int p_amount, object p_attacker);
}
