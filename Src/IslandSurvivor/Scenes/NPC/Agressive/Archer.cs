namespace IslandSurvivor.Scenes.NPC.Agressive;

using Godot;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Scenes.Projectiles;
using Core.Interfaces.Entities;

public partial class Archer : EnemyBase
{
    private PackedScene m_projectileScene;

    public override void _Ready()
    {
        Stats = GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");
        if (Stats == null)
        {
            GD.PrintErr("Archer node requires a StatManager child node.");
        }

        // Load the Arrow scene
        m_projectileScene = GD.Load<PackedScene>("res://Scenes/Projectiles/Arrow.tscn");
        if (m_projectileScene == null)
        {
            GD.PrintErr("Archer failed to load Arrow.tscn!");
        }

        base._Ready();
    }

    protected override void InitializeController()
    {
        // Use RangedController for the archer. We read the Exported StoppingDistance here.
        m_agressorController = new RangedController(StoppingDistance);
    }

    protected override void HandleAttackState()
    {
        // Archer attacks from a distance, not reliant on m_playersInHitbox like melee
        // We attack if we have line of sight and target is within shooting distance
        if (m_targetPlayer != null && m_agressorController is IRangedController controller)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);

            // Allow shooting if player is close enough (e.g. up to vision radius)
            // Assuming line of sight is true if target is not null and controller state says CHASE/ATTACK
            if (distanceToPlayer <= 250.0f && controller.CanAttack())
            {
                // Check direct line of sight again before shooting
                if (CheckLineOfSight())
                {
                    controller.StartAttack();
                    ShootProjectile();
                }
            }
        }
    }

    private void ShootProjectile()
    {
        if (m_projectileScene == null || m_targetPlayer == null) return;

        Node projectileNode = m_projectileScene.Instantiate();
        if (projectileNode is IProjectile projectile)
        {
            // Calculate direction to player
            Godot.Vector2 directionGodot = (m_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
            System.Numerics.Vector2 directionNumerics = new System.Numerics.Vector2(directionGodot.X, directionGodot.Y);
            System.Numerics.Vector2 startPositionNumerics = new System.Numerics.Vector2(GlobalPosition.X, GlobalPosition.Y);

            float damageAmount = Stats?.BaseDamage ?? 10.0f;

            projectile.Initialize(startPositionNumerics, directionNumerics, damageAmount, this);

            // Add projectile to the main scene (GetTree().CurrentScene) so it moves independently
            GetTree().CurrentScene.AddChild(projectileNode);

            projectile.Fire();
            GD.Print("[Archer] Arrow fired!");
        }
    }

    // Hitbox Area is unused for Archer attacks, but we can override it to avoid unnecessary logic
    protected override void OnHitboxAreaBodyEntered(Node2D p_body) { }
    protected override void OnHitboxAreaBodyExited(Node2D p_body) { }
}
