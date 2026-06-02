namespace IslandSurvivor.Interfaces;

public interface IInteractable
{
    float GetDistanceSquaredTo(float p_x, float p_y);
    void Interact();
    bool IsInteractable { get; }
    string InteractionPrompt { get; }
}
