namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;
using IslandSurvivor.Logic.Entities;

public partial class RangedAggressiveNpcBase : AggressiveNpcBase
{
    [Export] public PackedScene ProjectileScene { get; set; } = null!;

    [ExportGroup("Animations")]
    [Export] public string AttackAnimationName { get; set; } = "Attack";

    public override void _Ready()
    {
        base._Ready();

        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;
            m_attackController.AttackSprite = m_animatedSprite;
            m_attackController.ActionFrame = 5; // Arrow release frame

            m_attackController.AttackStarted += OnAttackStarted;
            m_attackController.AttackActionTriggered += ShootProjectile;
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
                    if (m_animatedSprite != null)
                    {
                        m_animatedSprite.FlipH = m_targetPlayer.GlobalPosition.X < GlobalPosition.X;
                    }
                    string direction = (m_animatedSprite != null && m_animatedSprite.FlipH) ? "Left" : "Right";
                    m_attackController.TryAttack(direction);
                }
            }
        }
    }

    protected virtual void OnAttackStarted()
    {
        PlayAttackAnimation(AttackAnimationName);
    }

    protected virtual void ShootProjectile()
    {
        if (ProjectileScene == null || m_targetPlayer == null) return;

        Node projectileNode = ProjectileScene.Instantiate();
        if (projectileNode is IProjectile projectile)
        {
            Godot.Vector2 directionGodot = (m_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
            Vector2 directionNumerics = new Vector2(directionGodot.X, directionGodot.Y);
            Vector2 startPositionNumerics = new Vector2(GlobalPosition.X, GlobalPosition.Y);

            float damageAmount = Stats?.BaseAttackValue ?? 10.0f;

            projectile.Initialize(startPositionNumerics, directionNumerics, damageAmount, this);

            GetTree().CurrentScene.AddChild(projectileNode);

            projectile.Fire();
        }
    }
}
