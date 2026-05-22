using Godot;
using System;
using IslandSurvivor.Scenes.Projectiles.EvilEye;
using IslandSurvivor.Scenes.Projectiles;
using IslandSurvivor.Logic.Projectiles;

namespace IslandSurvivor.Scenes;

public partial class TestProjectilesScene : Node2D
{
    private PackedScene _darkBlastScene;
    private PackedScene _evilEyeScene;
    private PackedScene _arrowScene;
    private PackedScene _orbScene;
    private PackedScene _scytheScene;

    private Node2D _player;
    private Timer _loopTimer;

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

        // Spawn Projectiles
        Vector2 projectileSpawnPoint = new Vector2(500, 100);
        Vector2 directionToPlayer = (_player.GlobalPosition - projectileSpawnPoint).Normalized();

        if (_arrowScene != null)
        {
            var arrow = _arrowScene.Instantiate<BaseProjectile>();
            AddChild(arrow);
            arrow.Initialize(projectileSpawnPoint, directionToPlayer, 10, this);
            arrow.Fire();
        }

        if (_orbScene != null)
        {
            var orb = _orbScene.Instantiate<BaseProjectile>();
            AddChild(orb);
            orb.Initialize(projectileSpawnPoint + new Vector2(-50, 0), directionToPlayer, 15, this);
            orb.Fire();
        }

        if (_scytheScene != null)
        {
            var scythe = _scytheScene.Instantiate<BaseProjectile>();
            AddChild(scythe);
            scythe.Initialize(projectileSpawnPoint + new Vector2(50, 0), directionToPlayer, 20, this);
            scythe.Fire();
        }
    }
}
