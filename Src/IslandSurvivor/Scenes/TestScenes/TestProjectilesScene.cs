using Godot;
using System;
using IslandSurvivor.Scenes.Projectiles.EvilEye;
using IslandSurvivor.Scenes.Projectiles;
using IslandSurvivor.Logic.Projectiles;

namespace IslandSurvivor.Scenes;

public partial class TestProjectilesScene : Node2D
{
    private PackedScene _darkBlastScene = null!;
    private PackedScene _evilEyeScene = null!;
    private PackedScene _arrowScene = null!;
    private PackedScene _orbScene = null!;
    private PackedScene _scytheScene = null!;

    private Node2D _player = null!;
    private Timer _loopTimer = null!;

    public override void _Ready()
    {
        base._Ready();

        _player = GetNodeOrNull<Node2D>("Player");
        if (_player != null)
        {
            if (_player.HasMethod("SetHealth"))
            {
                _player.Call("SetHealth", 5000);
            }
        }

        // Load projectile scenes
        _darkBlastScene = GD.Load<PackedScene>("res://Scenes/Projectiles/DarkBlast/DarkBlast.tscn");
        _evilEyeScene = GD.Load<PackedScene>("res://Scenes/Projectiles/EvilEye/EvilEye.tscn");
        _arrowScene = GD.Load<PackedScene>("res://Scenes/Projectiles/Arrow/Arrow.tscn");
        _orbScene = GD.Load<PackedScene>("res://Scenes/Projectiles/OrbProjectile/OrbProjectile.tscn");
        _scytheScene = GD.Load<PackedScene>("res://Scenes/Projectiles/DarkScythe/DarkScythe.tscn");

        // Start looping
        _loopTimer = new Timer();
        _loopTimer.WaitTime = 4.0f; // Wait 4 seconds between spawns
        _loopTimer.Autostart = true;
        _loopTimer.Timeout += SpawnAttacks;
        AddChild(_loopTimer);

        // Initial spawn
        SpawnAttacks();
    }

    private void SpawnAttacks()
    {
        if (_player == null) return;

        // Spawn DarkBlast
        if (_darkBlastScene != null)
        {
            var darkBlast = _darkBlastScene.Instantiate<Node2D>();
            darkBlast.GlobalPosition = new Vector2(200, 300);
            AddChild(darkBlast);
        }

        // Spawn EvilEye
        if (_evilEyeScene != null)
        {
            var evilEye = _evilEyeScene.Instantiate<EvilEye>();
            evilEye.GlobalPosition = new Vector2(800, 300);
            AddChild(evilEye);

            evilEye.Initialize(_player);
        }

        // Spawn Projectiles shooting straight down
        Vector2 spawnArrow = new Vector2(400, 100);
        Vector2 spawnOrb = new Vector2(500, 100);
        Vector2 spawnScythe = new Vector2(600, 100);
        Vector2 downwardDirection = Vector2.Down;

        if (_arrowScene != null)
        {
            var arrow = _arrowScene.Instantiate<BaseProjectile>();
            AddChild(arrow);
            arrow.Initialize(spawnArrow, downwardDirection, 10, this);
            arrow.Fire();
        }

        if (_orbScene != null)
        {
            var orb = _orbScene.Instantiate<BaseProjectile>();
            AddChild(orb);
            orb.Initialize(spawnOrb, downwardDirection, 15, this);
            orb.Fire();
        }

        if (_scytheScene != null)
        {
            var scythe = _scytheScene.Instantiate<BaseProjectile>();
            AddChild(scythe);
            scythe.Initialize(spawnScythe, downwardDirection, 20, this);
            scythe.Fire();
        }
    }
}
