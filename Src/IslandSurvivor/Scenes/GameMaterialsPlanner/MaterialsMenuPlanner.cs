using Godot;
using System;

public partial class MaterialsMenuPlanner : Control
{
	public override void _Ready()
	{
		//
	}

	public void _on_sell_materials_btn_pressed()
	{
        GD.Print("Bouton vendre ressources appuyé");
    }
}
