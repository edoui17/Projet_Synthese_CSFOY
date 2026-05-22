using Godot;
using System;
using IslandSurvivor.Scenes.Projectiles.EvilEye;



namespace IslandSurvivor.Scenes;

public partial class TestProjectilesScene : Node2D
{
    private PackedScene _darkBlastScene;
    private PackedScene _evilEyeScene;
    private Node2D _player;
    private Timer _loopTimer;

    public override void _Ready()
    {
        base._Ready();

        _player = GetNodeOrNull<Node2D>("Player");
        if (_player != null)
        {
            // Set max health directly to player if they have the method,
            // or just let it use default health for test since damage checks work.
            if (_player.HasMethod("SetHealth"))
            {
                _player.Call("SetHealth", 5000);
            }
        }

        // Load projectile scenes for looping
        _darkBlastScene = GD.Load<PackedScene>("res://Scenes/Projectiles/DarkBlast/DarkBlast.tscn");
        _evilEyeScene = GD.Load<PackedScene>("res://Scenes/Projectiles/EvilEye/EvilEye.tscn");

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

            // Initialize points it to the player
            evilEye.Initialize(_player);
        }
    }
}
