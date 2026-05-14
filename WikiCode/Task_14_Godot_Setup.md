# Task 14: Godot Editor Setup Instructions

Because the C# code relies on specific Node structures and configurations in the Godot Editor, please follow these steps to hook up the visual and audio feedback systems.

---

## 1. AudioManager Global Setup
We have created a new C# Singleton `AudioManager.cs` to handle all audio playback.

1. Go to **Project -> Project Settings -> Autoload**.
2. Add a new Node:
   - **Path:** `res://Src/IslandSurvivor/Globals/AudioManager.cs`
   - **Node Name:** `AudioManager`
   - Click **Add**.

---

## 2. Resource Hit/Destruction Particles (GPUParticles2D)
For trees, rocks, and gold, we need visual debris when they are destroyed.

1. Open your resource scenes (e.g., `AutomnTree.tscn`, `Rock.tscn`).
2. Add a `GPUParticles2D` node as a child of the root node.
   - **Name:** `DestructionParticles`
3. Configure the `DestructionParticles` properties:
   - **Emitting:** `Off` (Uncheck it)
   - **One Shot:** `On`
   - **Process Material:** Create a new `ParticleProcessMaterial`.
     - *Emission Shape:* Sphere (set radius to roughly the size of the object).
     - *Particle Flags:* Disable Z.
     - *Gravity:* Y = 98.
     - *Initial Velocity:* Randomize between 100 and 200.
     - *Scale:* Adjust curve to shrink over time.
   - **Texture:** Assign a small sprite representing debris (e.g., a leaf, a small rock, a splinter).
4. Save the scene. The C# script (`AutomnTree.cs`, etc.) will now automatically find `DestructionParticles` and emit them on death before queueing free.

*(Note: If the C# script queues free immediately, the particles will disappear. The updated C# scripts now detach the particles or delay `QueueFree` to let the effect play).*

---

## 3. Portal Shader and Audio Loop
The portal needs a visual activation effect and a looping ambient sound.

1. Open `Portal.tscn`.
2. **Audio:** Add an `AudioStreamPlayer2D` child node.
   - **Name:** `AmbientAudio`
   - **Stream:** Load your looping portal sound (prefer `.ogg`).
   - **Autoplay:** `Off`.
   - **Max Distance:** 500 (adjust so it fades nicely as the player walks away).
3. **Shader:** Select the Sprite/Mesh representing the portal portal surface.
   - Go to **Material** -> Create **New ShaderMaterial**.
   - Create a **New Shader**.
   - You can use a simple distortion or scrolling texture shader here.
   - Expose a parameter (e.g., `activation_intensity` or just rely on setting the material visibility).

The C# script (`PortalInteraction.cs`) will trigger `AmbientAudio.Play()` and fade it in using Tweeners when purchased.

---

## 4. Combat Audio Setup (Player & Enemies)
The `AudioManager` will handle playing sounds globally, so you do not *need* individual `AudioStreamPlayer` nodes on every enemy unless you want spatial 2D audio for hits.

If you want spatial audio (sounds coming from the enemy's location):
1. The `AudioManager` implementation provided uses a dynamic global approach.
2. Ensure you have loaded your sound files into the game folder (e.g., `res://Assets/Sounds/Combat/`).
3. (Optional but recommended) Map your sound paths in a centralized resource or just ensure the paths used in the C# `ExecuteAttack` or `TakeDamage` methods match where you put your assets.
