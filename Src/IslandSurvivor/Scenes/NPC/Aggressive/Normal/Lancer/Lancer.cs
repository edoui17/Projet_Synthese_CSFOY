namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;
using IslandSurvivor.Logic.Entities;
using Core.Managers.Stats;

public partial class Lancer : MeleeAggressiveNpcBase
{
    [Export] public float DashSpeedMultiplier { get; set; } = 5f;
    [Export] public float MinDashDistance { get; set; } = 200f;

    private ILancerController m_lancerController;
    private Area2D? m_dashActiveHitbox;
    private bool m_isDashing = false;
    private float m_windUpTimer = 0f;
    private float m_recoveryTimer = 0f;
    private float m_dashHitboxDelayTimer = 0f;
    private bool m_dashHitboxPending = false;

    private Area2D? m_hitboxAreaUp;
    private Area2D? m_hitboxAreaDown;

    public override void _Ready()
    {
        Stats = GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");
        if (Stats == null)
        {
            GD.PrintErr("Lancer node requires a StatManager child node.");
        }

        base._Ready();

        m_hitboxAreaUp = GetNodeOrNull<Area2D>("HitboxAreaUp");
        m_hitboxAreaDown = GetNodeOrNull<Area2D>("HitboxAreaDown");

        if (m_attackController != null)
        {
            m_attackController.ActionFrame = 3;
            if (m_hitboxAreaUp != null) m_attackController.RegisterArea("Up", m_hitboxAreaUp);
            if (m_hitboxAreaDown != null) m_attackController.RegisterArea("Down", m_hitboxAreaDown);
        }

        if (m_hitboxAreaRight != null) m_hitboxAreaRight.BodyEntered += OnDashHitboxEntered;
        if (m_hitboxAreaLeft != null) m_hitboxAreaLeft.BodyEntered += OnDashHitboxEntered;
        if (m_hitboxAreaUp != null) m_hitboxAreaUp.BodyEntered += OnDashHitboxEntered;
        if (m_hitboxAreaDown != null) m_hitboxAreaDown.BodyEntered += OnDashHitboxEntered;

        if (m_animatedSprite != null)
        {
            m_animatedSprite.AnimationFinished += OnAnimationFinished;
        }
    }

    protected override void InitializeController()
    {
        m_lancerController = new LancerController();
        m_lancerController.MinDashDistance = MinDashDistance;
        m_agressorController = (AgressorController)m_lancerController;
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_lancerController.CurrentState == NpcStates.DEAD) return;

        bool hasLineOfSight = CheckLineOfSight();
        float distanceSquaredToPlayer = float.MaxValue;

        if (m_targetPlayer != null)
        {
            distanceSquaredToPlayer = GlobalPosition.DistanceSquaredTo(m_targetPlayer.GlobalPosition);
            m_lancerController.UpdateTargetPositions(GlobalPosition, m_targetPlayer.GlobalPosition);
        }

        m_lancerController.UpdateDistanceToTarget(distanceSquaredToPlayer);
        m_lancerController.Update((float)p_delta, m_targetPlayer != null, hasLineOfSight);

        Vector2 direction = new Vector2(m_lancerController.CurrentDirection.X, m_lancerController.CurrentDirection.Y);
        float targetSpeed = IdleSpeed;

        string state = m_lancerController.CurrentState;

        if (m_attackController != null && m_attackController.IsAttacking)
        {
            // Driven by AttackController
            targetSpeed = 0f;
            direction = Vector2.Zero;
        }
        else if (state == LancerStates.WIND_UP)
        {
            targetSpeed = 0f;
            direction = Vector2.Zero;

            if (m_windUpTimer > 0f)
            {
                m_windUpTimer -= (float)p_delta;
                if (m_windUpTimer <= 0f)
                {
                    // Wind up finished, set dash direction and start dashing
                    if (m_targetPlayer != null)
                    {
                        Vector2 pPos = m_targetPlayer.GlobalPosition;
                        m_lancerController.UpdateChaseDirection(GlobalPosition, pPos);
                    }
                    m_lancerController.StartDash();
                }
            }
            else
            {
                // Initialize wind up timer
                m_windUpTimer = 0.75f;
            }
        }
        else if (state == LancerStates.RECOVERY)
        {
            targetSpeed = 0f;
            direction = Vector2.Zero;
            m_windUpTimer = 0f; // Reset wind up just in case

            if (m_recoveryTimer > 0f)
            {
                m_recoveryTimer -= (float)p_delta;
                if (m_recoveryTimer <= 0f)
                {
                    m_lancerController.FinishRecovery();
                }
            }
        }
        else if (state == LancerStates.MELEE)
        {
            targetSpeed = 0f;
            direction = Vector2.Zero;

            if (m_targetPlayer != null)
            {
                HandleAttackState();
            }
        }
        else if (state == LancerStates.DASHING)
        {
            if (!m_isDashing)
            {
                m_isDashing = true;

                string dashDirectionStr = "Right";
                if (m_targetPlayer != null)
                {
                    dashDirectionStr = GetDirectionString(m_targetPlayer.GlobalPosition);
                }

                // Cache the visual animation string so it doesn't change mid-dash if player moves
                string dashAnimName = "DashSide";
                if (dashDirectionStr == "Up") dashAnimName = "DashUp";
                else if (dashDirectionStr == "Down") dashAnimName = "DashDown";

                if (m_animatedSprite != null)
                {
                    m_animatedSprite.Play(dashAnimName);
                    // Also lock visual flip for Side dashes
                    if (dashDirectionStr == "Left") m_animatedSprite.FlipH = true;
                    else if (dashDirectionStr == "Right") m_animatedSprite.FlipH = false;
                }

                if (dashDirectionStr == "Left") m_dashActiveHitbox = m_hitboxAreaLeft;
                else if (dashDirectionStr == "Up") m_dashActiveHitbox = m_hitboxAreaUp;
                else if (dashDirectionStr == "Down") m_dashActiveHitbox = m_hitboxAreaDown;
                else m_dashActiveHitbox = m_hitboxAreaRight;

                // Delay hitbox activation by 0.2 seconds so the thrust has a travel visual
                m_dashHitboxDelayTimer = 0.2f;
                m_dashHitboxPending = true;
            }

            if (m_dashHitboxPending)
            {
                m_dashHitboxDelayTimer -= (float)p_delta;
                if (m_dashHitboxDelayTimer <= 0f)
                {
                    m_dashHitboxPending = false;
                    if (m_isDashing && m_dashActiveHitbox != null)
                    {
                        m_dashActiveHitbox.SetDeferred(Area2D.PropertyName.Monitoring, true);
                    }
                }
            }

            // Dash speed is ChaseSpeed * DashSpeedMultiplier
            targetSpeed = ChaseSpeed * DashSpeedMultiplier;
            direction = new Vector2(m_lancerController.CurrentDirection.X, m_lancerController.CurrentDirection.Y);
        }
        else if (state == NpcStates.CHASE && m_targetPlayer != null)
        {
            targetSpeed = ChaseSpeed;
            Vector2 globalPositionNumerics = GlobalPosition;
            Vector2 targetPositionNumerics = m_targetPlayer.GlobalPosition;
            m_lancerController.UpdateChaseDirection(globalPositionNumerics, targetPositionNumerics);
            direction = new Vector2(m_lancerController.CurrentDirection.X, m_lancerController.CurrentDirection.Y);
        }

        if (m_movementController != null)
        {
            m_movementController.Move(direction, targetSpeed);
        }
        else
        {
            Velocity = direction * targetSpeed;
            MoveAndSlide();
        }

        if (state == LancerStates.DASHING && GetSlideCollisionCount() > 0)
        {
            // Hit a wall during dash -> terminate dash
            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                KinematicCollision2D collision = GetSlideCollision(i);
                if (collision.GetCollider() is StaticBody2D or TileMap)
                {
                    EndDashSequence();
                    break;
                }
            }
        }

        if (state == NpcStates.IDLE && GetSlideCollisionCount() > 0)
        {
            m_lancerController.ForceNewDirection();
        }

        UpdateAnimation(new Vector2(m_lancerController.CurrentDirection.X, m_lancerController.CurrentDirection.Y));
    }

    private string GetDirectionString(Vector2 p_targetPosition)
    {
        Vector2 direction = p_targetPosition - GlobalPosition;
        if (System.Math.Abs(direction.Y) > System.Math.Abs(direction.X))
        {
            return direction.Y < 0 ? "Up" : "Down";
        }
        return direction.X < 0 ? "Left" : "Right";
    }

    protected override void HandleAttackState()
    {
        if (m_attackController != null && m_attackController.CanAttack && m_targetPlayer != null)
        {
            string attackDirectionStr = GetDirectionString(m_targetPlayer.GlobalPosition);

            string animName = "AttackSide";
            if (attackDirectionStr == "Up") animName = "AttackUp";
            else if (attackDirectionStr == "Down") animName = "AttackDown";

            if (m_animatedSprite != null)
            {
                if (attackDirectionStr == "Left") m_animatedSprite.FlipH = true;
                else if (attackDirectionStr == "Right") m_animatedSprite.FlipH = false;
            }

            m_attackController.SetAttackAnimation(animName);
            m_attackController.TryAttack(attackDirectionStr);
        }
    }

    protected override void OnAttackStarted()
    {
        if (m_animatedSprite != null && m_attackController != null)
        {
            m_animatedSprite.Play(m_attackController.AttackAnimationName);
            m_animatedSprite.Frame = 0;
        }
    }

    protected override void UpdateAnimation(Vector2 p_direction)
    {
        if (m_animatedSprite == null) return;

        bool isAttacking = m_attackController != null && m_attackController.IsAttacking;
        string state = m_lancerController.CurrentState;

        // Visual orientation
        if (p_direction.X != 0 && !isAttacking && state != LancerStates.WIND_UP && state != LancerStates.DASHING && state != LancerStates.RECOVERY)
        {
            m_animatedSprite.FlipH = p_direction.X < 0;
        }

        if (isAttacking)
        {
            string animName = m_attackController.AttackAnimationName;
            if (m_animatedSprite.Animation != animName)
            {
                m_animatedSprite.Play(animName);
                m_animatedSprite.Frame = 0;
            }
            return;
        }

        if (state == LancerStates.WIND_UP)
        {
            if (m_targetPlayer != null) m_animatedSprite.FlipH = m_targetPlayer.GlobalPosition.X < GlobalPosition.X;
            m_animatedSprite.Play("Idle");
            return;
        }
        else if (state == LancerStates.DASHING)
        {
            // Dash animation and flipping are locked in when dash starts to prevent desync
            // Just ensure it keeps playing whatever was locked in
            if (m_animatedSprite.Animation.ToString().StartsWith("Dash") == false)
            {
                m_animatedSprite.Play("DashSide"); // Fallback
            }
            return;
        }
        else if (state == LancerStates.RECOVERY)
        {
            m_animatedSprite.Play("Idle");
            return;
        }

        // Base movements
        if (Velocity.LengthSquared() > 0 && state != "Resting")
        {
            m_animatedSprite.Play("Moving");
        }
        else
        {
            m_animatedSprite.Play("Idle");
        }
    }

    private void OnAnimationFinished()
    {
        if (m_animatedSprite == null) return;

        string state = m_lancerController.CurrentState;

        if (state == LancerStates.DASHING && m_animatedSprite.Animation.ToString().StartsWith("Dash"))
        {
            // End of dash animation
            EndDashSequence();
        }
    }

    private void EndDashSequence()
    {
        m_isDashing = false;
        m_dashHitboxPending = false;
        if (m_dashActiveHitbox != null)
        {
            m_dashActiveHitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
            m_dashActiveHitbox = null;
        }
        m_lancerController.FinishDash();
        m_recoveryTimer = 0.8f;
    }

    private void OnDashHitboxEntered(Node2D p_body)
    {
        if (m_lancerController.CurrentState != LancerStates.DASHING) return;

        if (p_body.IsInGroup("Player") || p_body.Name == "Player")
        {
            // Hit the player!
            if (p_body is Core.Interfaces.Stats.IDamageable damageable)
            {
                float baseDamage = Stats?.BaseAttackValue ?? 10f;
                float attackStat = Stats?.GetCurrentValue(StatType.Attack) ?? 0f;
                int finalDamage = IslandSurvivor.Logic.CombatMath.CalculateDamage(baseDamage, attackStat);

                // Dash damage
                finalDamage = (int)(finalDamage * 1.5f);

                damageable.TakeDamage(finalDamage, this);
            }

            // Instantly truncate dash
            EndDashSequence();
        }
    }
}
