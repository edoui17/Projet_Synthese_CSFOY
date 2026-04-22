using Godot;
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

    private NavigationAgent2D m_navigationAgent;
    private AgressorController m_agressorController;
    private MovementController m_movementController;
    private Sprite2D m_sprite;
    private Node2D m_targetPlayer;
    private bool m_wasKilledByPlayer = false;

    public string CurrentState => m_agressorController?.CurrentState ?? NpcStates.IDLE;

    public override void _Ready()
    {
        m_agressorController = new AgressorController();

        m_navigationAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        m_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        m_movementController = GetNodeOrNull<MovementController>("MovementController");

        if (m_navigationAgent == null)
        {
            GD.PrintErr("Soldier node requires a NavigationAgent2D child node.");
        }
        else
        {
            m_navigationAgent.PathDesiredDistance = 4.0f;
            m_navigationAgent.TargetDesiredDistance = 4.0f;
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

        UpdateTarget();

        m_agressorController.Update((float)p_delta, m_targetPlayer != null);

        Vector2 direction = m_agressorController.CurrentDirection;
        float targetSpeed = IdleSpeed;

        if (m_agressorController.CurrentState == NpcStates.CHASE && m_targetPlayer != null)
        {
            targetSpeed = ChaseSpeed;
            if (m_navigationAgent != null)
            {
                m_navigationAgent.TargetPosition = m_targetPlayer.GlobalPosition;
                if (!m_navigationAgent.IsNavigationFinished())
                {
                    Vector2 nextPathPosition = m_navigationAgent.GetNextPathPosition();
                    direction = GlobalPosition.DirectionTo(nextPathPosition);
                    m_agressorController.UpdateChaseDirection(GlobalPosition, m_targetPlayer.GlobalPosition);
                }
            }
        }
        else if (m_agressorController.CurrentState == NpcStates.IDLE)
        {
            // Wandering
            if (m_navigationAgent != null)
            {
                Vector2 targetPos = GlobalPosition + (m_agressorController.CurrentDirection * 50f);
                m_navigationAgent.TargetPosition = targetPos;

                if (!m_navigationAgent.IsNavigationFinished())
                {
                    Vector2 nextPathPosition = m_navigationAgent.GetNextPathPosition();
                    direction = GlobalPosition.DirectionTo(nextPathPosition);
                }
                else
                {
                    direction = Vector2.Zero;
                    m_agressorController.ResetDirectionChangeTimer();
                }
            }
        }

        if (m_sprite != null && direction.X != 0)
        {
            m_sprite.FlipH = direction.X < 0;
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
    }

    private void UpdateTarget()
    {
        // Simple detection using group
        Godot.Collections.Array<Node> players = GetTree().GetNodesInGroup("Player");
        if (players.Count > 0)
        {
            Node2D playerNode = players[0] as Node2D;
            if (playerNode != null && GlobalPosition.DistanceTo(playerNode.GlobalPosition) <= DetectionRadius)
            {
                m_targetPlayer = playerNode;
            }
            else
            {
                m_targetPlayer = null;
            }
        }
        else
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
