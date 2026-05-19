import sys

def replace_in_file(filepath, search_str, replace_str):
    with open(filepath, 'r') as file:
        content = file.read()
    if search_str in content:
        content = content.replace(search_str, replace_str)
        with open(filepath, 'w') as file:
            file.write(content)
        print(f"Replaced successfully in {filepath}")
    else:
        print(f"Search string not found in {filepath}")

filepath = "./Src/IslandSurvivor/Scenes/Player/Player.cs"

# 1. Update TakeDamage
search_take_damage = """    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (m_movementController != null && m_movementController.IsDashing) return; // Invincible during dash

        if (Stats == null) return;

        float currentHealth = Stats.GetCurrentValue(StatType.Health);
        if (currentHealth <= 0) return;

        Stats.ModifyCurrentValue(StatType.Health, -p_amount);

        this.PlayHitFlash();
        this.PlayShake();"""

replace_take_damage = """    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (m_currentState == PlayerState.Dead) return;
        if (m_movementController != null && m_movementController.IsDashing) return; // Invincible during dash

        if (Stats == null) return;

        float currentHealth = Stats.GetCurrentValue(StatType.Health);
        if (currentHealth <= 0) return;

        Stats.ModifyCurrentValue(StatType.Health, -p_amount);

        currentHealth = Stats.GetCurrentValue(StatType.Health);

        if (currentHealth <= 0)
        {
            HandleDeath();
            return;
        }

        this.PlayHitFlash();
        this.PlayShake();"""

replace_in_file(filepath, search_take_damage, replace_take_damage)

# 2. Add HandleDeath
search_handle_death = """    private void OnInteractionAreaEntered(Area2D p_area)"""
replace_handle_death = """    private void HandleDeath()
    {
        SetState(PlayerState.Dead);
        Velocity = Vector2.Zero;

        // Play death sound (reusing hurt sound or specific death sound if available)
        AudioStream deathStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/player_hurt.wav"); // Fallback
        if (deathStream != null)
        {
            AudioManager.Instance?.PlaySound(deathStream);
        }

        if (m_animatedSprite != null)
        {
            // If there's a death animation, play it. Otherwise, stop animation.
            m_animatedSprite.Stop();
        }

        // Emit PlayerDiedEvent
        if (IslandSurvivor.Globals.ServiceRegistry.Instance != null && IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus != null)
        {
            IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Publish(new Core.Events.PlayerDiedEvent());
        }
    }

    private void OnInteractionAreaEntered(Area2D p_area)"""

replace_in_file(filepath, search_handle_death, replace_handle_death)

# 3. Prevent logic in _PhysicsProcess and _UnhandledInput if dead
search_physics = """    public override void _PhysicsProcess(double p_delta)
    {
        if (m_debugLabel != null)"""
replace_physics = """    public override void _PhysicsProcess(double p_delta)
    {
        if (m_currentState == PlayerState.Dead) return;

        if (m_debugLabel != null)"""

replace_in_file(filepath, search_physics, replace_physics)

search_input = """    public override void _UnhandledInput(InputEvent p_event)
    {
        if (p_event.IsActionPressed("attack"))"""
replace_input = """    public override void _UnhandledInput(InputEvent p_event)
    {
        if (m_currentState == PlayerState.Dead) return;

        if (p_event.IsActionPressed("attack"))"""

replace_in_file(filepath, search_input, replace_input)
