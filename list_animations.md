### Godot Animation Setup Checklist

Below is the list of animations you need to manually create inside the `AnimationPlayer` for each of the specific NPC scenes. The names must match exactly to trigger properly, although you can change the expected names in the Inspector for each State node if desired.

#### 1. Passive NPCs (e.g. Sheep)
- **Idle** (Waiting, doing nothing)
- **Moving** (Wandering around)
- **Flee** (Running away when hit)
- **Death** (Death animation)
- **Error** (Fallback animation, optional but recommended)

#### 2. Melee Enemies (e.g. Soldier)
- **Idle**
- **Moving** (Chasing player)
- **Attack** (Basic melee swing, needs to trigger `m_attackController` hitboxes if using the frame check)
- **Death**
- **Error** (Fallback)

#### 3. Ranged Enemies (e.g. Archer)
- **Idle**
- **Moving**
- **Attack** (Drawing/Shooting animation, used for the arrow release frame)
- **Death**
- **Error** (Fallback)

#### 4. Advanced Enemies (e.g. Lancer)
- **Idle**
- **Moving**
- **AttackSide** (Horizontal thrust)
- **AttackUp** (Vertical up thrust)
- **AttackDown** (Vertical down thrust)
- **DashSide** (Horizontal dash loop)
- **DashUp** (Vertical up dash loop)
- **DashDown** (Vertical down dash loop)
- **Death**
- **Error** (Fallback)

#### 5. Bosses (e.g. Reaper)
*Note: The Reaper has many custom phases from its script that should be mapped into Godot tracks.*
- **Idle** (Default float)
- **Moving** (Standard float chase)
- **IdleShielded** (Floating with shield visual active)
- **LosingShield** (Transition animation)
- **Shielding** (Transition animation)
- **MeleeAttackNormal**
- **MeleeAttackEnraged**
- **ScytheThrow**
- **RangeBlast**
- **TeleportIn**
- **TeleportOut**
- **Dying** (Mapped to the StateMachine's "Death" state, or rename the StateMachine's expected string to "Dying")
- **Error** (Fallback)
