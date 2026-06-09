namespace IslandSurvivor.Logic.StateMachine;

using Godot;
using IslandSurvivor.Logic.MagicSpells;
using System.Collections.Generic;

[GlobalClass]
public partial class MagicAttackState : AttackState
{
    [Export] public string AttackAnimationName { get; set; } = "Attack";
    [Export] public float MaxAttackRange { get; set; } = 350.0f;
    private bool m_spellInProgress = false;
    private MagicSpellNode m_currentSpell = null!;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);

        // Connect spell finished signals for all child spells
        foreach (Node child in GetChildren())
        {
            if (child is MagicSpellNode spell)
            {
                spell.SpellFinished += OnSpellFinished;
            }
        }
    }

    public override void Enter()
    {
        base.Enter();
        m_spellInProgress = false;

        if (m_hasCompleted) return;

        m_currentSpell = SelectRandomSpell();
        if (m_currentSpell == null)
        {
            m_hasCompleted = true;
            CompleteState(StateExitReason.Finished);
            return;
        }

        var (animSuffix, direction) = DetermineDirectionAndAnimation();
        string fullAnimName = $"{AttackAnimationName}{animSuffix}";

        if (AttackAnimationName.EndsWith("_Side") || AttackAnimationName.EndsWith("_Up") || AttackAnimationName.EndsWith("_Down"))
        {
            fullAnimName = AttackAnimationName;
        }
        else if (m_animationPlayer != null && !m_animationPlayer.HasAnimation(fullAnimName))
        {
            fullAnimName = AttackAnimationName;
        }

        var callable = new Callable(this, nameof(OnAttackActionTriggered));
        if (!m_attackController.IsConnected(IslandSurvivor.Nodes.AttackController.SignalName.AttackActionTriggered, callable))
        {
            m_attackController.Connect(IslandSurvivor.Nodes.AttackController.SignalName.AttackActionTriggered, callable);
        }

        m_attackController.SetAttackAnimation(fullAnimName);
        m_attackController.TryAttack(direction);
    }

    private MagicSpellNode SelectRandomSpell()
    {
        var spells = new List<MagicSpellNode>();
        float totalWeight = 0;

        foreach (Node child in GetChildren())
        {
            if (child is MagicSpellNode spell)
            {
                spells.Add(spell);
                totalWeight += spell.SelectionWeight;
            }
        }

        if (spells.Count == 0) return null!;
        if (totalWeight <= 0) return spells[GD.RandRange(0, spells.Count - 1)];

        float randomVal = (float)GD.RandRange(0.0, totalWeight);
        float currentSum = 0;

        foreach (var spell in spells)
        {
            currentSum += spell.SelectionWeight;
            if (randomVal <= currentSum)
            {
                return spell;
            }
        }

        return spells[^1]; // Fallback
    }

    public override void Exit()
    {
        base.Exit();
        if (m_attackController != null)
        {
            var callable = new Callable(this, nameof(OnAttackActionTriggered));
            if (m_attackController.IsConnected(IslandSurvivor.Nodes.AttackController.SignalName.AttackActionTriggered, callable))
            {
                m_attackController.Disconnect(IslandSurvivor.Nodes.AttackController.SignalName.AttackActionTriggered, callable);
            }
        }
    }

    public override void Update(double p_delta)
    {
        if (m_hasCompleted) return;

        if (m_attackController == null)
        {
            m_hasCompleted = true;
            CompleteState(StateExitReason.Finished);
            return;
        }

        // Wait for attack controller to finish the visual animation AND the spell to finish firing.
        if (!m_attackController.IsAttacking && !m_spellInProgress)
        {
            m_hasCompleted = true;
            Callable.From(() => CompleteState(StateExitReason.Finished)).CallDeferred();
        }
    }

    private void OnAttackActionTriggered()
    {
        if (m_currentSpell == null) return;

        m_spellInProgress = true;

        Vector2 targetPos = NpcContext.GlobalPosition + Vector2.Right;
        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc)
        {
            var target = aggNpc.GetTarget();
            if (GodotObject.IsInstanceValid(target))
            {
                targetPos = target.GlobalPosition;
            }
            else if (aggNpc.LockedDirection != Vector2.Zero)
            {
                targetPos = NpcContext.GlobalPosition + aggNpc.LockedDirection;
            }
        }

        m_currentSpell.Execute(NpcContext, targetPos);

        // Put the shooter node on cooldown so it respects standard attack rate delays.
        var shooter = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Shooter>("Shooter");
        if (shooter != null)
        {
            shooter.StartCooldown();
        }
    }

    private void OnSpellFinished()
    {
        m_spellInProgress = false;
    }
}
