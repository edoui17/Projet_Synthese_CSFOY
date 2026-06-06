namespace IslandSurvivor.Logic;

using Godot;
using IslandSurvivor.Logic.StateMachine;

[Tool]
public partial class AnimationTrackGenerator : Node
{
    [Export]
    AnimationPlayer animPlayer = null!;
    [Export]
    Node stateMachine = null!;
    [Export]
    public bool GenerateAnimations
    {
        get => false;
        set
        {
            if (value)
            {
                Generate();
            }
        }
    }

    private void Generate()
    {
        if (!Engine.IsEditorHint()) return;



        if (animPlayer == null)
        {
            GD.PrintErr("[AnimationTrackGenerator] Could not find an 'AnimationPlayer' node as a sibling.");
            return;
        }

        if (stateMachine == null)
        {
            GD.PrintErr("[AnimationTrackGenerator] Could not find a 'StateMachine' node as a sibling.");
            return;
        }

        // Create or get the default library
        AnimationLibrary library;
        if (animPlayer.HasAnimationLibrary(""))
        {
            library = animPlayer.GetAnimationLibrary("");
        }
        else
        {
            library = new AnimationLibrary();
            animPlayer.AddAnimationLibrary("", library);
        }

        int addedCount = 0;

        foreach (Node child in stateMachine.GetChildren())
        {
            if (child is State state)
            {
                // We use reflection to grab AnimationName if it exists since it's defined on the subclasses
                var animProp = child.Get("AnimationName");
                if (animProp.VariantType == Variant.Type.String)
                {
                    string animName = animProp.AsString();
                    if (!string.IsNullOrEmpty(animName) && !library.HasAnimation(animName))
                    {
                        var anim = new Animation();
                        library.AddAnimation(animName, anim);
                        addedCount++;
                        GD.Print($"[AnimationTrackGenerator] Added animation: {animName}");
                    }
                }

                var fallbackProp = child.Get("FallbackAnimationName");
                if (fallbackProp.VariantType == Variant.Type.String)
                {
                    string fallbackName = fallbackProp.AsString();
                    if (!string.IsNullOrEmpty(fallbackName) && !library.HasAnimation(fallbackName))
                    {
                        var anim = new Animation();
                        library.AddAnimation(fallbackName, anim);
                        addedCount++;
                        GD.Print($"[AnimationTrackGenerator] Added fallback animation: {fallbackName}");
                    }
                }
            }
        }

        if (addedCount > 0)
        {
            GD.Print($"[AnimationTrackGenerator] Successfully added {addedCount} animations. Please configure their tracks manually.");
        }
        else
        {
            GD.Print("[AnimationTrackGenerator] No new animations needed. All requested animations already exist.");
        }
    }
}
