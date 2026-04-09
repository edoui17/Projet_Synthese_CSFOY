using Core.Managers.Stats;
using Godot;
using System;

public partial class MaterialsMenuPlanner : Control
{
	public override void _Ready()
	{
		//
	}

	public void _on_buy_next_island_btn_pressed()
	{
        GD.Print("Bouton pour acheter nouvelle ile appuyé");

        // Instead of hardcoding relative paths, query from the top of the scene
        var navMenu = GetTree().Root.GetNodeOrNull<IslandSurvivor.Scenes.NavigationMenu.NavigationMenu>("Main/BaseMapIsland/NavigationMenu");

        if (navMenu != null)
        {
            navMenu.OpenMenu();
            Visible = false; // Hide this menu temporarily
        }
        else
        {
            // Fallback for tests/other scene hierarchies
            navMenu = GetNodeOrNull<IslandSurvivor.Scenes.NavigationMenu.NavigationMenu>("../../NavigationMenu");
            if (navMenu != null)
            {
                navMenu.OpenMenu();
                Visible = false;
            }
            else
            {
                GD.PrintErr("NavigationMenu node not found! Make sure it is instantiated correctly.");
            }
        }
    }

	public void _on_buy_luck_btn_pressed()
	{
		GD.Print("Bouton pour acheter de la chance appuyé");
        //TryPurchaseUpgrade("Or", StatType.Luck, ref m_luckUpgradeCount);
    }

    public void _on_buy_attack_btn_pressed()
	{
        GD.Print("Bouton pour acheter de l'attaque appuyé");
        //TryPurchaseUpgrade("Roche", StatType.Attack, ref m_attackUpgradeCount);
    }

	public void _on_buy_speed_btn_pressed()
	{
		GD.Print("Bouton pour acheter de la vitesse appuyé");
        //TryPurchaseUpgrade("Bois", StatType.Speed, ref m_speedUpgradeCount);
    }

	public void _on_buy_health_btn_pressed()
	{
		GD.Print("Bouton pour acheter de la vie appuyé");
        //TryPurchaseUpgrade("Viande", StatType.Health, ref m_healthUpgradeCount);
    }
}
