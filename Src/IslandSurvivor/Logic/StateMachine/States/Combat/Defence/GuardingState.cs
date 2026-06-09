namespace IslandSurvivor.Logic.StateMachine;

using Godot;

public partial class GuardingState : State
{
    [ExportGroup("Guard Configuration")]
    [Export] public float GuardChance { get; set; } = 0.3f;
    [Export] public float GuardDuration { get; set; } = 1.0f;
    [Export] public float GuardCooldown { get; set; } = 5.0f;
    [Export] public float DamageMultiplier { get; set; } = 0.0f; // 0 means take 0 damage, 1 means full damage

    [ExportGroup("Guard Animations")]
    [Export] public string GuardAnimationName { get; set; } = "Guard";

    private AnimationPlayer m_animationPlayer = null!;
    private Sprite2D m_sprite = null!;
    private float m_guardTimer;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Enter()
    {
        base.Enter();
        m_guardTimer = GuardDuration;

        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
        {
            npc.Velocity = Vector2.Zero;
        }

        string guardDirectionStr = "Right";

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc)
        {
            var target = aggNpc.GetTarget();
            if (target != null)
            {
                // Always flip correctly and block only left/right based on prompt
                bool isTargetLeft = target.GlobalPosition.X < NpcContext.GlobalPosition.X;
                guardDirectionStr = isTargetLeft ? "Left" : "Right";

                if (m_sprite != null)
                {
                    m_sprite.FlipH = isTargetLeft;
                }
            }
            else
            {
                if (m_sprite != null)
                {
                    guardDirectionStr = m_sprite.FlipH ? "Left" : "Right";
                }
            }
        }
        else
        {
            if (m_sprite != null)
            {
                guardDirectionStr = m_sprite.FlipH ? "Left" : "Right";
            }
        }

        if (NpcContext.HasMethod("SetGuardState"))
        {
            NpcContext.Call("SetGuardState", true, guardDirectionStr, DamageMultiplier);
        }

        string fullAnimName = $"{GuardAnimationName}_Side"; // Since only side guarding exists

        if (m_animationPlayer != null)
        {
            if (m_animationPlayer.HasAnimation(fullAnimName))
            {
                m_animationPlayer.Play(fullAnimName);
            }
            else if (m_animationPlayer.HasAnimation(GuardAnimationName))
            {
                m_animationPlayer.Play(GuardAnimationName);
            }
            else if (m_animationPlayer.HasAnimation(FallbackAnimationName))
            {
                m_animationPlayer.Play(FallbackAnimationName);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (NpcContext.HasMethod("SetGuardState"))
        {
            NpcContext.Call("SetGuardState", false, "", 1.0f);
        }

        if (NpcContext.HasMethod("StartGuardCooldown"))
        {
            NpcContext.Call("StartGuardCooldown", GuardCooldown);
        }
    }

    public override void PhysicsUpdate(double p_delta)
    {
        base.PhysicsUpdate(p_delta);

        m_guardTimer -= (float)p_delta;

        if (m_guardTimer <= 0)
        {
            CompleteState(StateExitReason.Finished);
            return;
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc)
        {
            var target = aggNpc.GetTarget();
            if (target != null)
            {
                float distSq = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);

                float maxAttackRange = 350f;
                if (StateMachine.TryGetState<RangedAttackState>(out var ranged)) maxAttackRange = ranged.MaxAttackRange;
                if (StateMachine.TryGetState<MagicAttackState>(out var magic)) maxAttackRange = System.Math.Max(maxAttackRange, magic.MaxAttackRange);
                float maxAttackSq = maxAttackRange * maxAttackRange;


                // If the player goes out of combat zone, exit GuardingState
                if (distSq > maxAttackSq)
                {
                    CompleteState(StateExitReason.Finished);
                    return;
                }

                // Continuously face the player
                bool isTargetLeft = target.GlobalPosition.X < NpcContext.GlobalPosition.X;
                if (m_sprite != null)
                {
                    m_sprite.FlipH = isTargetLeft;
                }

                string guardDirectionStr = isTargetLeft ? "Left" : "Right";
                if (NpcContext.HasMethod("SetGuardState"))
                {
                    NpcContext.Call("SetGuardState", true, guardDirectionStr, DamageMultiplier);
                }
            }
        }
    }
}
