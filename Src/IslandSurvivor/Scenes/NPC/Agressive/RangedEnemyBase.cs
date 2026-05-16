namespace IslandSurvivor.Scenes.NPC.Agressive;

using Godot;
using Core.Interfaces.Entities;
using IslandSurvivor.Logic.Entities;

public abstract partial class RangedEnemyBase : EnemyBase
{
    [Export] public PackedScene ProjectileScene { get; set; }
    protected IslandSurvivor.Nodes.Combat.AttackController? m_attackController;

    public override void _Ready()
    {
        base._Ready();

        m_attackController = GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;
            m_attackController.AttackStarted += ShootProjectile;
        }
        else
        {
            GD.PushWarning($"{Name}: AttackController not found.");
        }
    }

    protected override void InitializeController()
    {
        StoppingDistance = 250.0f;
        m_agressorController = new RangedController(StoppingDistance);
    }

    protected override void HandleAttackState()
    {
        if (m_targetPlayer != null && m_attackController != null && m_attackController.CanAttack)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);

            if (distanceToPlayer <= StoppingDistance)
            {
                if (CheckLineOfSight())
                {
                    // For Ranged attacks, direction area isn't strictly needed if we just spawn a projectile,
                    // but we can pass a dummy direction or register a dummy area if required by TryAttack.
                    // Assuming we pass any string since TryAttack might check for it.
                    // Wait, TryAttack requires a registered area. We can either override TryAttack for ranged,
                    // or just manage a fake Area2D or simply bypass the area requirement.
                    // Since AttackController requires a registered direction, let's create a dummy area or use a generic method.
                    // Actually, let's just add a method to trigger attack without direction in AttackController, or just register a dummy one.
                    // Let's call TryAttack with "Ranged" and ensure a dummy area is registered if needed, or update TryAttack.
                    string direction = (m_animatedSprite != null && m_animatedSprite.FlipH) ? "Left" : "Right";
                    m_attackController.TryAttack(direction);
                }
            }
        }
    }

    protected virtual void ShootProjectile()
    {
        if (ProjectileScene == null || m_targetPlayer == null) return;

        Node projectileNode = ProjectileScene.Instantiate();
        if (projectileNode is IProjectile projectile)
        {
            // Calculate direction to player
            Godot.Vector2 directionGodot = (m_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
            System.Numerics.Vector2 directionNumerics = new System.Numerics.Vector2(directionGodot.X, directionGodot.Y);
            System.Numerics.Vector2 startPositionNumerics = new System.Numerics.Vector2(GlobalPosition.X, GlobalPosition.Y);

            float damageAmount = Stats?.BaseAttackValue ?? 10.0f;

            projectile.Initialize(startPositionNumerics, directionNumerics, damageAmount, this);

            GetTree().CurrentScene.AddChild(projectileNode);

            projectile.Fire();
        }
    }
}
