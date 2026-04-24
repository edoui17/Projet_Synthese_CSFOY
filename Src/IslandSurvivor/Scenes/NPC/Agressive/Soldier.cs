using Godot;
using System;
using Core.Domain;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes.Movement;
using Core.Interfaces;
using IslandSurvivor.Nodes;
using Core.Managers.Stats;

public partial class Soldier : CharacterBody2D, INpc, IEnemy, IDamageable
{
    [Export] public StatManager Stats { get; set; }

    [Export] public string NpcType { get; set; } = "Agressive";
    [Export] public string EnemyType { get; set; } = "Soldier";

    [Export] public float IdleSpeed { get; set; } = 30.0f;
    [Export] public float ChaseSpeed { get; set; } = 150.0f;
    [Export] public float DetectionRadius { get; set; } = 250.0f;

    private AgressorController m_agressorController;
    private MovementController m_movementController;
    private AnimatedSprite2D m_animatedSprite;
    private Node2D m_targetPlayer;
    private bool m_wasKilledByPlayer = false;
    private Area2D m_detectionArea;
    private RayCast2D m_lineOfSightRay;

    public string CurrentState => m_agressorController?.CurrentState ?? NpcStates.IDLE;

    public override void _Ready()
    {
        m_agressorController = new AgressorController();

        m_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        m_movementController = GetNodeOrNull<MovementController>("MovementController");
        m_detectionArea = GetNodeOrNull<Area2D>("DetectionArea");
        m_lineOfSightRay = GetNodeOrNull<RayCast2D>("LineOfSightRay");

        if (m_animatedSprite == null)
        {
            GD.PrintErr("Soldier node requires an AnimatedSprite2D child node.");
        }

        if (m_movementController == null)
        {
            GD.PrintErr("Soldier node requires a MovementController child node.");
        }

        if (m_detectionArea == null)
        {
            GD.PrintErr("Soldier node requires an Area2D child node named 'DetectionArea'.");
        }
        else
        {
            // Connect to Area2D signals. We use Callable.From to ensure correct typing.
            m_detectionArea.BodyEntered += OnDetectionAreaBodyEntered;
            m_detectionArea.BodyExited += OnDetectionAreaBodyExited;
        }

        if (m_lineOfSightRay == null)
        {
            GD.PrintErr("Soldier node requires a RayCast2D child node named 'LineOfSightRay'.");
        }

        if (Stats != null)
        {
            Stats.SetCurrentValue(StatType.Health, 10);
            // Example for base damage. Can use StatType.Attack if it exists in StatType
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_agressorController.CurrentState == NpcStates.DEAD) return;

        bool hasLineOfSight = CheckLineOfSight();

        m_agressorController.Update((float)p_delta, m_targetPlayer != null, hasLineOfSight);

        Vector2 direction = m_agressorController.CurrentDirection;
        float targetSpeed = IdleSpeed;

        if (m_agressorController.CurrentState == NpcStates.CHASE && m_targetPlayer != null)
        {
            targetSpeed = ChaseSpeed;
            m_agressorController.UpdateChaseDirection(GlobalPosition, m_targetPlayer.GlobalPosition);
            direction = m_agressorController.CurrentDirection;
        }

        // Apply movement
        if (m_movementController != null)
        {
            m_movementController.Move(direction, targetSpeed);
        }
        else
        {
            Velocity = direction * targetSpeed;
            MoveAndSlide();
        }

        // Obstacle avoidance in IDLE state
        if (m_agressorController.CurrentState == NpcStates.IDLE && GetSlideCollisionCount() > 0)
        {
            m_agressorController.ForceNewDirection();
        }

        UpdateAnimation(direction);
    }

    private void UpdateAnimation(Vector2 p_direction)
    {
        if (m_animatedSprite == null) return;

        if (Velocity.LengthSquared() > 0)
        {
            m_animatedSprite.Play("Moving");
        }
        else
        {
            m_animatedSprite.Play("Idle");
        }

        if (p_direction.X != 0)
        {
            m_animatedSprite.FlipH = p_direction.X < 0;
        }
    }

    private bool CheckLineOfSight()
    {
        if (m_targetPlayer == null || m_lineOfSightRay == null)
            return false;

        // Point the RayCast towards the target
        Vector2 targetDirection = m_targetPlayer.GlobalPosition - GlobalPosition;
        m_lineOfSightRay.TargetPosition = targetDirection;
        m_lineOfSightRay.ForceRaycastUpdate();

        // If it's not colliding, the line of sight is clear.
        // Or if it's colliding with the player itself (though typically the mask should only check walls).
        if (m_lineOfSightRay.IsColliding())
        {
            GodotObject collider = m_lineOfSightRay.GetCollider();
            if (collider is Node2D node && node.IsInGroup("Player"))
            {
                return true;
            }
            return false; // Hit a wall or something else
        }

        return true; // No obstacles
    }

    private void OnDetectionAreaBodyEntered(Node2D p_body)
    {
        if (p_body.IsInGroup("Player"))
        {
            m_targetPlayer = p_body;
        }
    }

    private void OnDetectionAreaBodyExited(Node2D p_body)
    {
        if (p_body == m_targetPlayer)
        {
            m_targetPlayer = null;
        }
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (m_agressorController.CurrentState == NpcStates.DEAD) return;

        if (Stats != null)
        {
            float currentHp = Stats.GetCurrentValue(StatType.Health);
            Stats.SetCurrentValue(StatType.Health, currentHp - p_amount);
        }

        bool isDead = Stats == null || Stats.GetCurrentValue(StatType.Health) <= 0;

        if (p_attacker is Node2D attackerNode)
        {
            if (isDead && attackerNode.IsInGroup("Player"))
            {
                m_wasKilledByPlayer = true;
            }

            if (!isDead)
            {
                // Force target to whoever hit it
                m_targetPlayer = attackerNode;

                Modulate = new Color(1, 0.5f, 0.5f);
                GetTree().CreateTimer(0.2f).Timeout += () =>
                {
                    if (IsInstanceValid(this)) Modulate = Colors.White;
                };
            }
        }

        if (isDead)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        m_agressorController.SetDead();

        if (m_wasKilledByPlayer)
        {
            int goldAmount = 2;
            ResourceItem goldResource = new ResourceItem("gold_coin", "Piece d'Or", "Gold Coin", "res://Assets/TinySwords/TinySwords(Update010)/Resources/Gold_Coin.png");

            if (SignalManager.Instance != null)
            {
                SignalManager.Instance.EmitMaterialDestroyed(this, goldResource, goldAmount);
                GD.Print($"Soldier died. Sent {goldAmount} gold to inventory.");
            }
        }

        QueueFree();
    }
}
