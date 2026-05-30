using System.Collections.Generic;
using IslandSurvivor.Interfaces;

namespace IslandSurvivor.Managers;

public interface IInteractionService
{
    IInteractable? GetBestInteractable(float p_playerX, float p_playerY, IEnumerable<IInteractable> p_interactables);
}

public class InteractionService : IInteractionService
{
    public IInteractable? GetBestInteractable(float p_playerX, float p_playerY, IEnumerable<IInteractable> p_interactables)
    {
        IInteractable? bestTarget = null;
        float minDistanceSquared = float.MaxValue;

        foreach (var interactable in p_interactables)
        {
            if (!interactable.IsInteractable) continue;

            float distanceSquared = interactable.GetDistanceSquaredTo(p_playerX, p_playerY);
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                bestTarget = interactable;
            }
        }

        return bestTarget;
    }
}
