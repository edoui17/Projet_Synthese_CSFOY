import re

with open('./Src/IslandSurvivor/Scenes/NPC/Agressive/Archer.cs', 'r') as f:
    content = f.read()

# Update stopping distance
old_init = """    protected override void InitializeController()
    {
        // Use RangedController for the archer. We read the Exported StoppingDistance here.
        m_agressorController = new RangedController(StoppingDistance);
    }"""

new_init = """    protected override void InitializeController()
    {
        StoppingDistance = 250.0f;
        m_agressorController = new RangedController(StoppingDistance);
    }"""
content = content.replace(old_init, new_init)


# Update attack state
old_handle = """    protected override void HandleAttackState()
    {
        // Archer attacks from a distance, not reliant on m_playersInHitbox like melee
        // We attack if we have line of sight and target is within shooting distance
        if (m_targetPlayer != null && m_agressorController is IRangedController controller)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);

            // Allow shooting if player is close enough (e.g. up to vision radius)
            // Assuming line of sight is true if target is not null and controller state says CHASE/ATTACK
            if (distanceToPlayer <= 250.0f && controller.CanAttack())
            {
                // Check direct line of sight again before shooting
                if (CheckLineOfSight())
                {
                    controller.StartAttack();
                    ShootProjectile();
                }
            }
        }
    }"""

new_handle = """    protected override async void HandleAttackState()
    {
        if (m_targetPlayer != null && m_agressorController is IRangedController controller)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);

            if (distanceToPlayer <= 250.0f)
            {
                if (controller.CanAttack() && CheckLineOfSight())
                {
                    controller.StartAttack();

                    await ToSignal(GetTree().CreateTimer(0.4f), SceneTreeTimer.SignalName.Timeout);
                    if (m_agressorController.CurrentState == NpcStates.DEAD) return;

                    ShootProjectile();
                }
            }
        }
    }"""
content = content.replace(old_handle, new_handle)

with open('./Src/IslandSurvivor/Scenes/NPC/Agressive/Archer.cs', 'w') as f:
    f.write(content)
