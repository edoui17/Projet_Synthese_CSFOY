namespace Core.Interfaces;

public interface IDamageable
{
    string NpcType { get; set; }

    void TakeDamage(int p_amount, object p_attacker);
}
