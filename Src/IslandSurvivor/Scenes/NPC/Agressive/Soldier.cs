using Godot;
using System;
using Core.Domain;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes.Movement;
using Core.Interfaces;
using Core.Interfaces.Stats;
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
    [Export] public float StoppingDistance { get; set; } = 82.0f;

    private AgressorController m_agressorController;
    private MovementController m_movementController;
    private AnimatedSprite2D m_animatedSprite;
    private Node2D m_targetPlayer;
    private bool m_wasKilledByPlayer = false;
    private Area2D m_detectionArea;
    private RayCast2D m_lineOfSightRay;

    public string CurrentState => m_agressorController?.CurrentState ?? NpcStates.IDLE;

    private object? m_lastAttacker;

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
            Stats.Connect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
    }

    private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        if ((StatType)p_statType == StatType.Health && p_currentValue <= 0)
        {
            if (Stats != null)
            {
                Stats.Disconnect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
            }
            HandleDeath(m_lastAttacker);
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_agressorController.CurrentState == NpcStates.DEAD) return;

        bool hasLineOfSight = CheckLineOfSight();
        string previousState = m_agressorController.CurrentState;

        m_agressorController.Update((float)p_delta, m_targetPlayer != null, hasLineOfSight);

        if (m_agressorController.CurrentState == NpcStates.CHASE && previousState != NpcStates.CHASE)
        {
            GD.Print("[Soldier] Ligne de vue confirmee, debut de la poursuite !");
        }
        else if (m_agressorController.CurrentState != NpcStates.CHASE && previousState == NpcStates.CHASE)
        {
            GD.Print("[Soldier] Cible perdue de vue, abandon de la poursuite.");
        }

        Vector2 direction = m_agressorController.CurrentDirection;
        float targetSpeed = IdleSpeed;

        if (m_agressorController.CurrentState == NpcStates.CHASE && m_targetPlayer != null)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);
            if (distanceToPlayer <= StoppingDistance)
            {
                targetSpeed = 0f;
                direction = Vector2.Zero;
            }
            else
            {
                targetSpeed = ChaseSpeed;
                m_agressorController.UpdateChaseDirection(GlobalPosition, m_targetPlayer.GlobalPosition);
                direction = m_agressorController.CurrentDirection;
            }
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

        // Point the RayCast towards the target using local coordinates relative to the RayCast2D
        // The Soldier script is attached to CharacterBody2D, so ToLocal converts the target's
        // global position into coordinates relative to the Soldier (and thus the RayCast, assuming it's positioned at 0,0).
        Vector2 targetLocalPosition = ToLocal(m_targetPlayer.GlobalPosition);

        // RayCast TargetPosition is relative to the RayCast's position
        m_lineOfSightRay.TargetPosition = targetLocalPosition;

        // Force an update to get immediate collision results
        m_lineOfSightRay.ForceRaycastUpdate();

        // If it's colliding with something
        if (m_lineOfSightRay.IsColliding())
        {
            GodotObject collider = m_lineOfSightRay.GetCollider();

            // If it hit the player directly, we have line of sight
            if (collider is Node2D node && (node.IsInGroup("Player") || node.Name == "Player"))
            {
                return true;
            }

            // If we hit something else (like a wall on Mask 1), it blocks the view.
            return false;
        }

        // If the ray cast doesn't collide with ANYTHING, it means the player is out of reach of the ray,
        // OR the ray only checks walls (Mask 1) and didn't hit any wall.
        // If the ray is long enough to reach the player, and hits nothing, the path is clear.
        return true;
    }

    private void OnDetectionAreaBodyEntered(Node2D p_body)
    {
        // Check group or name as fallback to ensure the player is detected
        if (p_body.IsInGroup("Player") || p_body.Name == "Player")
        {
            GD.Print("[Soldier] Joueur detecte dans l'Area2D !");
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

        m_lastAttacker = p_attacker;

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

                IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);
            }
        }
    }

    private void HandleDeath(object p_attacker = null)
    {
        m_agressorController.SetDead();

        // Notify ScoreManager to add score points
        if (IslandSurvivor.Globals.ServiceRegistry.Instance != null)
        {
            int goldAmount = 2;

            float luck = 0f;
            if (p_attacker is Node GodotAttacker)
            {
                StatManager attackerStats = GodotAttacker.GetNodeOrNull<StatManager>("StatManager");
                if (attackerStats != null)
                {
                    luck = attackerStats.GetCurrentValue(StatType.Luck);
                }
            }

            float bonusChance = luck * 0.05f;
            int bonusQuantity = (int)bonusChance;
            float fractionalChance = bonusChance - bonusQuantity;

            Random random = new();
            if (random.NextDouble() < fractionalChance)
            {
                bonusQuantity++;
            }

            goldAmount += bonusQuantity;

            ResourceItem goldResource = new ResourceItem("gold_coin", "Piece d'Or", "Gold Coin", "res://Assets/TinySwords/TinySwords(Update010)/Resources/Gold_Coin.png");

            if (SignalManager.Instance != null)
            {
                SignalManager.Instance.EmitMaterialDestroyed(this, goldResource, goldAmount);
                GD.Print($"Soldier died. Sent {goldAmount} gold to inventory.");
            }
            IslandSurvivor.Globals.ServiceRegistry.Instance.ScoreTracker.AddScore(10); // Example score value
        }

        QueueFree();
    }
}
