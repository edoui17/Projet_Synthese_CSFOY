namespace IslandSurvivor.Interfaces;

public interface IInteractable
{
    float GetDistanceTo(float p_x, float p_y);
    void Interact();
    bool IsInteractable { get; }
    string InteractionPrompt { get; }
}
