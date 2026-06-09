namespace IslandSurvivor.Logic.MagicSpells;

using Godot;
using IslandSurvivor.Logic;
using System.Collections.Generic;

[GlobalClass]
public partial class OrbAttackSpell : MagicSpellNode
{
    [Export] public PackedScene ProjectileScene { get; set; } = null!;
    [Export] public float Damage { get; set; } = 20.0f;

    [ExportGroup("Patterns Allowed")]
    [Export] public bool AllowSingle { get; set; } = true;
    [Export] public bool AllowBurst { get; set; } = true;
    [Export] public bool AllowArc { get; set; } = true;
    [Export] public bool AllowCircle { get; set; } = true;
    [Export] public bool AllowProgressiveArc { get; set; } = true;

    [ExportGroup("Burst / Progressive Config")]
    [Export] public int BurstCount { get; set; } = 3;
    [Export] public float BurstInterval { get; set; } = 0.2f;
    [Export] public int ProgressiveArcCount { get; set; } = 5;
    [Export] public float ProgressiveArcInterval { get; set; } = 0.15f;
    [Export] public float ProgressiveArcAngleSpan { get; set; } = 90.0f; // Total arc in degrees

    [ExportGroup("Arc / Circle Config")]
    [Export] public int ArcCount { get; set; } = 3;
    [Export] public float ArcAngleSpan { get; set; } = 60.0f; // Total arc in degrees
    [Export] public int CircleCount { get; set; } = 8;

    private enum OrbPattern
    {
        Single,
        Burst,
        Arc,
        Circle,
        ProgressiveArc
    }

    private Node2D m_shooter = null!;
    private Vector2 m_baseDirection;

    private int m_shotsFired = 0;
    private int m_shotsTotal = 0;
    private float m_timer = 0.0f;
    private float m_interval = 0.0f;
    private bool m_isActive = false;
    private OrbPattern m_currentPattern;

    public override void _PhysicsProcess(double p_delta)
    {
        if (!m_isActive) return;

        m_timer -= (float)p_delta;
        if (m_timer <= 0)
        {
            FireNextShot();
            m_timer = m_interval;
        }
    }

    public override void Execute(Node2D p_shooter, Vector2 p_targetPos)
    {
        m_shooter = p_shooter;
        m_baseDirection = (p_targetPos - p_shooter.GlobalPosition).Normalized();
        if (m_baseDirection == Vector2.Zero)
        {
            m_baseDirection = Vector2.Right;
            if (p_shooter is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc && aggNpc.LockedDirection != Vector2.Zero)
            {
                m_baseDirection = aggNpc.LockedDirection;
            }
        }

        m_currentPattern = ChooseRandomPattern();

        m_shotsFired = 0;
        m_timer = 0;

        switch (m_currentPattern)
        {
            case OrbPattern.Single:
                m_shotsTotal = 1;
                m_interval = 0;
                break;
            case OrbPattern.Burst:
                m_shotsTotal = BurstCount;
                m_interval = BurstInterval;
                break;
            case OrbPattern.Arc:
                m_shotsTotal = 1; // Handled in one tick
                m_interval = 0;
                break;
            case OrbPattern.Circle:
                m_shotsTotal = 1; // Handled in one tick
                m_interval = 0;
                break;
            case OrbPattern.ProgressiveArc:
                m_shotsTotal = ProgressiveArcCount;
                m_interval = ProgressiveArcInterval;
                break;
        }

        m_isActive = true;
    }

    private OrbPattern ChooseRandomPattern()
    {
        var validPatterns = new List<OrbPattern>();
        if (AllowSingle) validPatterns.Add(OrbPattern.Single);
        if (AllowBurst) validPatterns.Add(OrbPattern.Burst);
        if (AllowArc) validPatterns.Add(OrbPattern.Arc);
        if (AllowCircle) validPatterns.Add(OrbPattern.Circle);
        if (AllowProgressiveArc) validPatterns.Add(OrbPattern.ProgressiveArc);

        if (validPatterns.Count == 0)
        {
            return OrbPattern.Single; // Fallback
        }

        int index = GD.RandRange(0, validPatterns.Count - 1);
        return validPatterns[index];
    }

    private void FireNextShot()
    {
        if (m_shotsFired >= m_shotsTotal)
        {
            FinishSpell();
            return;
        }

        switch (m_currentPattern)
        {
            case OrbPattern.Single:
            case OrbPattern.Burst:
                SpawnProjectile(m_baseDirection);
                break;

            case OrbPattern.Arc:
                float arcStartAngle = m_baseDirection.Angle() - Mathf.DegToRad(ArcAngleSpan / 2f);
                float arcAngleStep = ArcCount > 1 ? Mathf.DegToRad(ArcAngleSpan) / (ArcCount - 1) : 0;
                for (int i = 0; i < ArcCount; i++)
                {
                    Vector2 dir = Vector2.Right.Rotated(arcStartAngle + (i * arcAngleStep));
                    SpawnProjectile(dir);
                }
                break;

            case OrbPattern.Circle:
                float circleAngleStep = Mathf.Tau / CircleCount;
                for (int i = 0; i < CircleCount; i++)
                {
                    Vector2 dir = Vector2.Right.Rotated(i * circleAngleStep);
                    SpawnProjectile(dir);
                }
                break;

            case OrbPattern.ProgressiveArc:
                float progStartAngle = m_baseDirection.Angle() - Mathf.DegToRad(ProgressiveArcAngleSpan / 2f);
                float progAngleStep = ProgressiveArcCount > 1 ? Mathf.DegToRad(ProgressiveArcAngleSpan) / (ProgressiveArcCount - 1) : 0;
                Vector2 progDir = Vector2.Right.Rotated(progStartAngle + (m_shotsFired * progAngleStep));
                SpawnProjectile(progDir);
                break;
        }

        m_shotsFired++;

        if (m_shotsFired >= m_shotsTotal)
        {
            FinishSpell();
        }
    }

    private void SpawnProjectile(Vector2 p_direction)
    {
        if (ProjectileScene == null || m_shooter == null) return;

        Node projectileNode = ProjectileScene.Instantiate();
        if (projectileNode is not IProjectile projectile)
        {
            projectileNode.QueueFree();
            return;
        }

        // Use the shooter's shooter node spawn position if available
        Vector2 startPosition = m_shooter.GlobalPosition;
        var shooterNode = m_shooter.GetNodeOrNull<IslandSurvivor.Nodes.Shooter>("Shooter");
        if (shooterNode != null && shooterNode.SpawnPosition != null)
        {
            startPosition = shooterNode.SpawnPosition.GlobalPosition;
        }

        float damageAmount = Damage;
        if (m_shooter is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc)
        {
            damageAmount = aggNpc.Stats?.BaseAttackValue ?? Damage;
        }

        projectile.Initialize(startPosition, p_direction, damageAmount, m_shooter);
        GetTree().CurrentScene.AddChild(projectileNode);
        projectile.Fire();
    }

    private void FinishSpell()
    {
        m_isActive = false;
        EmitSignal(SignalName.SpellFinished);
    }
}
