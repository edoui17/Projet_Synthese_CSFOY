namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;
using IslandSurvivor.Logic.Entities;
using Core.Managers.Stats;

public partial class Lancer : MeleeAggressiveNpcBase
{
    private ILancerController m_lancerController;
    private Area2D m_dashBodyArea;
    private bool m_isDashing = false;
    private float m_windUpTimer = 0f;
    private float m_recoveryTimer = 0f;

    public override void _Ready()
    {
        Stats = GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");
        if (Stats == null)
        {
            GD.PrintErr("Lancer node requires a StatManager child node.");
        }

        base._Ready();

        m_dashBodyArea = GetNodeOrNull<Area2D>("DashBodyArea");
        if (m_dashBodyArea != null)
        {
            m_dashBodyArea.Monitoring = false; // Disabled by default
            m_dashBodyArea.BodyEntered += OnDashBodyEntered;
        }
        else
        {
            GD.PushWarning($"{Name}: DashBodyArea not found.");
        }

        if (m_animatedSprite != null)
        {
            // For loops on specific animations
            m_animatedSprite.AnimationFinished += OnAnimationFinished;
        }
    }

    protected override void InitializeController()
    {
        m_lancerController = new LancerController();
        m_agressorController = (AgressorController)m_lancerController;
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_lancerController.CurrentState == NpcStates.DEAD) return;

        bool hasLineOfSight = CheckLineOfSight();
        float distanceToPlayer = float.MaxValue;

        if (m_targetPlayer != null)
        {
            distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);
            m_lancerController.UpdateTargetPositions(GlobalPosition, m_targetPlayer.GlobalPosition);
        }

        m_lancerController.UpdateDistanceToTarget(distanceToPlayer);
        m_lancerController.Update((float)p_delta, m_targetPlayer != null, hasLineOfSight);

        UpdateAnimation(new Vector2(m_lancerController.CurrentDirection.X, m_lancerController.CurrentDirection.Y));

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
                m_windUpTimer = 0.5f;
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
                if (m_dashBodyArea != null) m_dashBodyArea.Monitoring = true;
            }

            // Dash speed is ChaseSpeed * 3
            targetSpeed = ChaseSpeed * 3f;
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
    }

    protected override void HandleAttackState()
    {
        if (m_attackController != null && m_attackController.CanAttack && m_targetPlayer != null)
        {
            // Attack controller will handle cooldowns
            string direction = (m_animatedSprite != null && m_animatedSprite.FlipH) ? "Left" : "Right";
            m_attackController.TryAttack(direction);
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
            if (m_animatedSprite.Animation != "Attack")
            {
                m_animatedSprite.Play("Attack");
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
            m_animatedSprite.Play("Dash");
            return;
        }
        else if (state == LancerStates.RECOVERY)
        {
            m_animatedSprite.Play("Idle");
            return;
        }

        // Base movements
        if (Velocity.LengthSquared() > 0)
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

        if (state == LancerStates.DASHING && m_animatedSprite.Animation == "Dash")
        {
            // End of dash animation
            EndDashSequence();
        }
    }

    private void EndDashSequence()
    {
        m_isDashing = false;
        if (m_dashBodyArea != null) m_dashBodyArea.Monitoring = false;
        m_lancerController.FinishDash();
        m_recoveryTimer = 0.8f;
    }

    private void OnDashBodyEntered(Node2D p_body)
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
