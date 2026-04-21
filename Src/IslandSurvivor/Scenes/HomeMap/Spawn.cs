using Godot;

public partial class Spawn : Marker2D
{
    [Export] public Marker2D SpawnPoint { get; set; }
    [Export] public PackedScene PlayerScene { get; set; }

    private IslandSurvivor.Managers.TreePopulationManager m_treeManager;

    public override void _Ready()
    {
        GD.Print("Spawn._Ready() exécuté");

        if (PlayerScene == null)
        {
            GD.PrintErr("PlayerScene n'est pas assigné !");
            return;
        }

        if (SpawnPoint == null)
        {
            GD.PrintErr("SpawnPoint n'est pas assigné !");
            return;
        }
        GD.Print("Juste avant CallDeferred");

        m_treeManager = new IslandSurvivor.Managers.TreePopulationManager();
        AddChild(m_treeManager);
        m_treeManager.PopulateTrees();

        CallDeferred(nameof(SpawnPlayer));
        GD.Print("Juste après CallDeferred");

    }

    private void SpawnPlayer()
    {
        GD.Print("SpawnPlayer appelé !");

        var instance = PlayerScene.Instantiate();
        GD.Print("Instance créée :", instance);

        if (instance is not Node2D player)
        {
            GD.PrintErr("La scène PlayerScene n'a pas un Node2D en racine !");
            return;
        }

        player.Position = SpawnPoint.GlobalPosition;
        GD.Print("Position du joueur :", player.Position);

        GetParent().AddChild(player);
        GD.Print("Joueur ajouté au parent :", GetParent());
    }
}
