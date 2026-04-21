using Godot;
using Core.Managers.Stats;
using IslandSurvivor.Nodes;

namespace IslandSurvivor.Nodes.Movement;

public partial class MovementController : Node
{
    [Export] public float BaseSpeed { get; set; } = 300f;
    [Export] public StatManager? Stats { get; set; }

    private CharacterBody2D? m_parentBody;

    public override void _Ready()
    {
        base._Ready();

        m_parentBody = GetParent() as CharacterBody2D;

        if (m_parentBody == null)
        {
            GD.PushWarning("MovementController: Parent is not a CharacterBody2D.");
        }
    }

    public void Move(Vector2 p_direction, float? p_customBaseSpeed = null)
    {
        if (m_parentBody == null) return;

        float baseSpeedToUse = p_customBaseSpeed ?? BaseSpeed;
        float speedMultiplier = 1.0f;

        if (Stats != null)
        {
            float speedStat = Stats.GetCurrentValue(StatType.Speed);
            // Treat the stat as a percentage increase. For example, 10 means +10% speed.
            speedMultiplier += (speedStat / 100f);
        }

        float finalSpeed = baseSpeedToUse * speedMultiplier;

        m_parentBody.Velocity = p_direction * finalSpeed;
        m_parentBody.MoveAndSlide();
    }
}
