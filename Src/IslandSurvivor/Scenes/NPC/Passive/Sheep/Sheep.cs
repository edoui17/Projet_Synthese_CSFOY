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

public partial class Sheep : CharacterBody2D, INpc, IDamageable
{
    [Export] public StatManager Stats { get; set; } = null!;

    [Export] public string NpcType { get; set; } = "Passive";
    [Export] public float IdleSpeed { get; set; } = 30.0f;
    [Export] public float FleeSpeed { get; set; } = 120.0f;

    private PassiveController m_passiveController = null!;
    [Export] private MovementController m_movementController = null!;
    [Export] private AnimatedSprite2D m_sprite = null!;
    private bool m_wasKilledByPlayer = false;

    public string CurrentState => m_passiveController?.CurrentState ?? NpcStates.IDLE;

    private object? m_lastAttacker;

    public override void _Ready()
    {
        m_passiveController = new PassiveController();

        if (Stats != null)
        {
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
        if (m_passiveController.CurrentState == NpcStates.DEAD) return;

        m_passiveController.Update((float)p_delta);

        @Vector2 direction = m_passiveController.CurrentDirection;
        float targetSpeed = IdleSpeed;

        if (m_passiveController.CurrentState == NpcStates.FLEE)
        {
            targetSpeed = FleeSpeed;
        }

        if (m_sprite != null)
        {
            if (direction.X != 0)
            {
                m_sprite.FlipH = direction.X < 0;
            }

            if (m_passiveController.CurrentState == NpcStates.FLEE)
            {
                if (m_sprite.Animation != "FLEE") m_sprite.Play("FLEE");
            }
            else
            {
                if (Velocity.LengthSquared() > 0 || direction.LengthSquared() > 0)
                {
                    if (m_sprite.Animation != "IDLE") m_sprite.Play("IDLE");
                }
                else
                {
                    if (m_sprite.Animation != "IDLE") m_sprite.Play("IDLE");
                }
            }
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

        if (m_passiveController.CurrentState == NpcStates.IDLE && GetSlideCollisionCount() > 0)
        {
            m_passiveController.ForceNewDirection();
        }
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (m_passiveController.CurrentState == NpcStates.DEAD) return;

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
                m_passiveController.StartFleeing(GlobalPosition, attackerNode.GlobalPosition);

                IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);
                IslandSurvivor.Extensions.NodeExtensions.PlayShake(this);

                AudioStream hurtStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/animal_hurt.wav");
                if (hurtStream != null)
                {
                    IslandSurvivor.Globals.AudioManager.Instance?.PlaySound2D(hurtStream, GlobalPosition);
                }
            }
        }
    }

    private void HandleDeath(object? p_attacker = null)
    {
        m_passiveController.SetDead();

        if (IslandSurvivor.Globals.ServiceRegistry.Instance != null)
        {
            IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Publish(new Core.Events.EnemyKilledEvent(Name, NpcType, 0f));
        }

        // Le drop est desormais genere systematiquement a la mort
        Random random = new();
        int meatAmount = random.Next(1, 4); // 1 a 3 viandes de base

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

        if (random.NextDouble() < fractionalChance)
        {
            bonusQuantity++;
        }

        meatAmount += bonusQuantity;

        ResourceItem meatResource = new ResourceItem("meat_01", "Viande", "Meat", "res://Assets/TinySwords/Terrain/Meat Resource/Meat Resource.png");

        Node2D targetNode = p_attacker as Node2D;
        @Vector2 fallbackPosition = targetNode != null ? targetNode.GlobalPosition : GlobalPosition;

        PackedScene dropScene = GD.Load<PackedScene>("res://Scenes/Ressources/ResourceDrop.tscn");
        if (dropScene != null)
        {
            for (int i = 0; i < meatAmount; i++)
            {
                if (dropScene.Instantiate() is IslandSurvivor.Scenes.Ressources.ResourceDrop drop)
                {
                    drop.Initialize(meatResource, 1, GlobalPosition, targetNode, fallbackPosition);
                    GetParent().AddChild(drop);
                }
            }
        }
        else
        {
            if (SignalManager.Instance != null)
            {
                SignalManager.Instance.EmitMaterialDestroyed(this, meatResource, meatAmount);
                GD.Print($"Sheep died. Sent {meatAmount} meat to inventory via SignalManager.");
            }
        }

        AudioStream deathStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/animal_death.wav");
        if (deathStream != null)
        {
            IslandSurvivor.Globals.AudioManager.Instance?.PlaySound2D(deathStream, GlobalPosition);
        }

        QueueFree();
    }
}