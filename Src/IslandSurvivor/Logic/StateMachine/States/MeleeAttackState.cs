namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class MeleeAttackState : State
{
    [Export] public string AttackAnimationName { get; set; } = "Attack";

    public override bool IsActionState => true;

    private IslandSurvivor.Nodes.Combat.AttackController m_attackController = null!;
    private Sprite2D m_sprite = null!;
    private bool m_hasCompleted = false;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Enter()
    {
        base.Enter();
        m_hasCompleted = false;

        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
        {
            npc.Velocity = Vector2.Zero;
        }

        if (m_attackController != null && m_attackController.CanAttack)
        {
            string animSuffix = "_Side";
            string direction = "Right";

            bool handledTargetDirection = false;

            if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
            {
                var target = aggNpc.GetTarget();
                if (GodotObject.IsInstanceValid(target))
                {
                    // Lock direction toward target immediately to prevent jitter during attack animation
                    Vector2 safeDir = NpcContext.GlobalPosition.DirectionTo(target.GlobalPosition);
                    if (safeDir != Vector2.Zero)
                    {
                        aggNpc.LockedDirection = safeDir;
                    }
                    Vector2 toTarget = aggNpc.LockedDirection;

                    if (System.Math.Abs(toTarget.Y) > System.Math.Abs(toTarget.X))
                    {
                        if (toTarget.Y < 0)
                        {
                            animSuffix = "_Up";
                            direction = "Up";
                        }
                        else
                        {
                            animSuffix = "_Down";
                            direction = "Down";
                        }
                    }
                    else
                    {
                        animSuffix = "_Side";
                        direction = toTarget.X < 0 ? "Left" : "Right";
                        if (m_sprite != null)
                        {
                            m_sprite.FlipH = toTarget.X < 0;
                        }
                    }

                    handledTargetDirection = true;
                }
            }

            if (!handledTargetDirection)
            {
                // Fallback for non-aggressive NPCs or missing target
                if (m_sprite != null)
                {
                    direction = m_sprite.FlipH ? "Left" : "Right";
                }
            }

            string fullAnimName = $"{AttackAnimationName}{animSuffix}";

            // Check if the base AttackAnimationName already has the suffix to prevent double appending if configured wrongly
            if (AttackAnimationName.EndsWith("_Side") || AttackAnimationName.EndsWith("_Up") || AttackAnimationName.EndsWith("_Down"))
            {
                fullAnimName = AttackAnimationName;
            }

            m_attackController.SetAttackAnimation(fullAnimName);
            m_attackController.TryAttack(direction);
        }
        else
        {
            if (!m_hasCompleted)
            {
                m_hasCompleted = true;
                CompleteState(StateExitReason.Finished);
            }
        }
    }

  // Inside MeleeAttackState.cs, modify the Update method:
  public override void Update(double p_delta)
  {
    if (m_hasCompleted) return;

    if (m_attackController != null)
    {
      // Add a safety check: only complete if the controller is NOT attacking
      // AND it isn't in the middle of a cleanup frame.
      if (!m_attackController.IsAttacking)
      {
        // Optional: Add a frame delay to let the physics server catch up
        // or simply ensure the state is fully stable.
        m_hasCompleted = true;

        // Use call_deferred to ensure the transition happens AFTER 
        // the current frame's physics/animation tasks are processed
        Callable.From(() => CompleteState(StateExitReason.Finished)).CallDeferred();
      }
    }
    else
    {
      m_hasCompleted = true;
      CompleteState(StateExitReason.Finished);
    }
  }
}
