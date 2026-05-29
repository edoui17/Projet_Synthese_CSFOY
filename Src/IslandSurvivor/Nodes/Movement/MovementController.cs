using Godot;
using Core.Managers;
using IslandSurvivor.Nodes;

namespace IslandSurvivor.Nodes;

public partial class MovementController : Node
{
    [Export] public StatManager? Stats { get; set; }

    [ExportGroup("Dash Settings")]
    [Export] public float DashCooldown { get; set; } = 3.0f;
    [Export] public float DashDuration { get; set; } = 0.2f;
    [Export] public float DashSpeedMultiplier { get; set; } = 3.0f;

    private CharacterBody2D? m_parentBody;

    public bool IsDashing { get; private set; } = false;
    public float TimeSinceLastDash { get; private set; } = 3.0f;

    public bool IsStunned { get; private set; } = false;

    private float m_dashTimer = 0f;
    private Vector2 m_dashDirection = Vector2.Zero;
    private float m_stunTimer = 0f;

    public override void _Ready()
    {
        base._Ready();

        m_parentBody = GetParent() as CharacterBody2D;
        TimeSinceLastDash = DashCooldown;

        if (m_parentBody == null)
        {
            GD.PushWarning("MovementController: Parent is not a CharacterBody2D.");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        float fDelta = (float)delta;

        if (IsStunned)
        {
            m_stunTimer -= fDelta;
            if (m_stunTimer <= 0f)
            {
                IsStunned = false;
            }
        }

        if (TimeSinceLastDash < DashCooldown)
        {
            TimeSinceLastDash += fDelta;
            if (TimeSinceLastDash > DashCooldown) TimeSinceLastDash = DashCooldown;
        }

        if (IsDashing)
        {
            HandleDash(fDelta);
        }
    }

    public void Move(Vector2 p_direction, float? p_customBaseSpeed = null)
    {
        if (m_parentBody == null || IsDashing) return;

        if (IsStunned)
        {
            m_parentBody.Velocity = Vector2.Zero;
            m_parentBody.MoveAndSlide();
            return;
        }

        float baseSpeedToUse = p_customBaseSpeed ?? Stats?.BaseSpeedValue ?? 300f;

        // 1 stat point = +5% speed
        float speedStat = Stats?.GetCurrentValue(StatType.Speed) ?? 0f;
        float finalSpeed = baseSpeedToUse * (1f + (speedStat * 0.05f));

        m_parentBody.Velocity = p_direction * finalSpeed;
        m_parentBody.MoveAndSlide();
    }

    public bool TryDash(Vector2 p_direction)
    {
        if (IsDashing || IsStunned || TimeSinceLastDash < DashCooldown) return false;

        IsDashing = true;
        TimeSinceLastDash = 0f;
        m_dashTimer = 0f;
        m_dashDirection = p_direction;

        return true;
    }

    public void CancelDash()
    {
        IsDashing = false;
        m_dashTimer = 0f;
    }

    public void ApplyStun(float p_duration)
    {
        IsStunned = true;
        if (m_stunTimer < p_duration)
        {
            m_stunTimer = p_duration;
        }
    }

    private void HandleDash(float p_delta)
    {
        if (m_parentBody == null) return;

        m_dashTimer += p_delta;

        float baseSpeed = Stats?.BaseSpeedValue ?? 300f;
        float finalSpeed = baseSpeed * DashSpeedMultiplier;

        m_parentBody.Velocity = m_dashDirection * finalSpeed;
        m_parentBody.MoveAndSlide();

        if (m_dashTimer >= DashDuration)
        {
            IsDashing = false;
        }
    }
}
