using Godot;
using System;
using IslandSurvivor.Scenes.Projectiles;
using IslandSurvivor.Scenes.Projectiles;
using IslandSurvivor.Logic;

namespace IslandSurvivor.Scenes;

public partial class TestProjectilesScene : Node2D
{
    private PackedScene m_darkBlastScene = null!;
    private PackedScene m_evilEyeScene = null!;
    private PackedScene m_arrowScene = null!;
    private PackedScene m_orbScene = null!;
    private PackedScene m_scytheScene = null!;

    private Node2D m_player = null!;
    private Timer m_loopTimer = null!;

    public override void _Ready()
    {
        base._Ready();

        m_player = GetNodeOrNull<Node2D>("Player");
        if (m_player != null && m_player.HasMethod("SetHealth"))
        {
            m_player.Call("SetHealth", 5000);
        }

        // Load projectile scenes
        m_darkBlastScene = GD.Load<PackedScene>("res://Scenes/Projectiles/DarkBlast/DarkBlast.tscn");
        m_evilEyeScene = GD.Load<PackedScene>("res://Scenes/Projectiles/EvilEye/EvilEye.tscn");
        m_arrowScene = GD.Load<PackedScene>("res://Scenes/Projectiles/Arrow/Arrow.tscn");
        m_orbScene = GD.Load<PackedScene>("res://Scenes/Projectiles/OrbProjectile/OrbProjectile.tscn");
        m_scytheScene = GD.Load<PackedScene>("res://Scenes/Projectiles/DarkScythe/DarkScythe.tscn");

        // Start looping
        m_loopTimer = new Timer();
        m_loopTimer.WaitTime = 4.0f; // Wait 4 seconds between spawns
        m_loopTimer.Autostart = true;
        m_loopTimer.Timeout += SpawnAttacks;
        AddChild(m_loopTimer);

        // Initial spawn
        SpawnAttacks();
    }

    private void SpawnAttacks()
    {
        if (m_player == null) return;

        // Spawn DarkBlast
        if (m_darkBlastScene != null)
        {
            var darkBlast = m_darkBlastScene.Instantiate<Node2D>();
            darkBlast.GlobalPosition = new Vector2(200, 300);
            AddChild(darkBlast);
        }

        // Spawn EvilEye
        if (m_evilEyeScene != null)
        {
            var evilEye = m_evilEyeScene.Instantiate<EvilEye>();
            evilEye.GlobalPosition = new Vector2(800, 300);
            AddChild(evilEye);

            evilEye.Initialize(m_player);
        }

        // Spawn Projectiles shooting straight down
        Vector2 spawnArrow = new Vector2(400, 100);
        Vector2 spawnOrb = new Vector2(500, 100);
        Vector2 spawnScythe = new Vector2(600, 100);
        Vector2 downwardDirection = Vector2.Down;

        if (m_arrowScene != null)
        {
            var arrow = m_arrowScene.Instantiate<BaseProjectile>();
            AddChild(arrow);
            arrow.Initialize(spawnArrow, downwardDirection, 10, this);
            arrow.Fire();
        }

        if (m_orbScene != null)
        {
            var orb = m_orbScene.Instantiate<BaseProjectile>();
            AddChild(orb);
            orb.Initialize(spawnOrb, downwardDirection, 15, this);
            orb.Fire();
        }

        if (m_scytheScene != null)
        {
            var scythe = m_scytheScene.Instantiate<BaseProjectile>();
            AddChild(scythe);
            scythe.Initialize(spawnScythe, downwardDirection, 20, this);
            scythe.Fire();
        }
    }
}
