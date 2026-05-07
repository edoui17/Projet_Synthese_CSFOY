# Editor Instructions: Soldier Combat System (US 6.3)

To finalize the combat system integration for the Soldier, follow these steps in the Godot Editor:

1. **Fix Soldier Hitbox Area**:
   - Open `res://Scenes/NPC/Agressive/Soldier.tscn`.
   - The user reported an error with `HitboxArea`. Ensure there is an `Area2D` named **exactly** `HitboxArea` as a child of the `Soldier` root node.
   - Add a `CollisionShape2D` as a child of `HitboxArea`.
   - Assign a new `CircleShape2D` (or adjust the shape points as seen in your screenshot) to the `CollisionShape2D`. Ensure the warning disappears.
   - Position this shape in front of the Soldier to represent its melee attack range.

2. **Configure HitboxArea Collision Layers**:
   - Select the `HitboxArea` node.
   - Go to the Inspector -> `CollisionObject2D` -> `Collision`.
   - Set **Layer** to nothing (or a dedicated enemy attack layer if you have one, but it's an Area so it just needs to monitor).
   - Set **Mask** to `3` (Player Layer) so it can detect the player entering its range.

3. **Verify AnimatedSprite2D**:
   - Select the `AnimatedSprite2D` node on the `Soldier`.
   - Ensure an animation named exactly `Attack` exists (case-sensitive as per the code `m_animatedSprite.Play("Attack");`).
   - Confirm that the `Moving` and `Idle` animations exist and are spelled correctly.
