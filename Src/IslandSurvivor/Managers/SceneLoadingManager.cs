using Godot;
using System;
using IslandSurvivor.Globals;
using IslandSurvivor.Globals;

namespace IslandSurvivor.Managers;

public partial class SceneLoadingManager : CanvasLayer
{
    public static SceneLoadingManager Instance { get; private set; } = null!;
    private Control m_loadingUI = null!;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
        Layer = 100; // Render on top of everything

        m_loadingUI = new ColorRect
        {
            Color = new Color(0, 0, 0, 1),
            Visible = false
        };
        m_loadingUI.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        var label = new Label
        {
            Text = "Loading...",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        m_loadingUI.AddChild(label);
        AddChild(m_loadingUI);
    }

    public void ShowLoading()
    {
        if (m_loadingUI != null) m_loadingUI.Visible = true;
    }

    public void HideLoading()
    {
        if (m_loadingUI != null) m_loadingUI.Visible = false;
    }

    public async void LoadScene(string p_scenePath)
    {
        ShowLoading();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        ChangeScene(p_scenePath);
    }

    private void ChangeScene(string p_scenePath)
    {
        var error = GetTree().ChangeSceneToFile(p_scenePath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[SceneLoadingManager] Failed to load scene {p_scenePath}. Error: {error}");
        }
        HideLoading();
    }
}
