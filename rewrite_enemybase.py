import re

with open('./Src/IslandSurvivor/Scenes/NPC/Agressive/EnemyBase.cs', 'r') as f:
    content = f.read()

# Fix UpdateAnimation call removing it from end of physics process
content = content.replace("m_agressorController.ForceNewDirection();\n        }\n\n        UpdateAnimation(direction);", "m_agressorController.ForceNewDirection();\n        }")

# Replace HandleAttackState
old_handle = """    protected virtual void HandleAttackState()
    {
        // Override in child classes for specific attack behavior
        if (m_playersInHitbox.Count > 0 && m_agressorController is IAgressorController controller && controller.CanAttack())
        {
            controller.StartAttack();
            int damageAmount = (int)(Stats?.BaseDamage ?? 5);
            foreach (var player in m_playersInHitbox)
            {
                player.TakeDamage(damageAmount, this);
            }
        }
    }"""

new_handle = """    protected virtual void UpdateHitboxDirection()
    {
        if (m_animatedSprite != null)
        {
            if (m_hitboxAreaRight != null) m_hitboxAreaRight.Monitoring = !m_animatedSprite.FlipH;
            if (m_hitboxAreaLeft != null) m_hitboxAreaLeft.Monitoring = m_animatedSprite.FlipH;
        }
    }

    protected virtual async void HandleAttackState()
    {
        // Override in child classes for specific attack behavior
        if (m_playersInHitbox.Count > 0 && m_agressorController is IAgressorController controller && controller.CanAttack())
        {
            controller.StartAttack();

            // Wind-up delay
            await ToSignal(GetTree().CreateTimer(0.4f), SceneTreeTimer.SignalName.Timeout);
            if (m_agressorController.CurrentState == NpcStates.DEAD) return;

            int damageAmount = (int)(Stats?.BaseDamage ?? 5);
            var playersToDamage = new System.Collections.Generic.List<IDamageable>(m_playersInHitbox);
            foreach (var player in playersToDamage)
            {
                player.TakeDamage(damageAmount, this);
            }
        }
    }"""

content = content.replace(old_handle, new_handle)

with open('./Src/IslandSurvivor/Scenes/NPC/Agressive/EnemyBase.cs', 'w') as f:
    f.write(content)
