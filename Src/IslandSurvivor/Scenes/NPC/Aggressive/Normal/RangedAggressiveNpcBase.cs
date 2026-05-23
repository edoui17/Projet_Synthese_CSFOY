namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Globals;

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

            m_attackController.AttackStarted += OnAttackStarted;
            m_attackController.AttackActionTriggered += ShootProjectile;
        }
        else
        {
            GD.PushWarning($"{Name}: AttackController not found.");
        }
    }

    protected virtual void OnAttackStarted()
    {
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
