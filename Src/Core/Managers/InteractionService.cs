using System.Collections.Generic;
using Core.Interfaces;

namespace Core.Managers;

public interface IInteractionService
{
    IInteractable? GetBestInteractable(float p_playerX, float p_playerY, IEnumerable<IInteractable> p_interactables);
}

public class InteractionService : IInteractionService
{
    public IInteractable? GetBestInteractable(float p_playerX, float p_playerY, IEnumerable<IInteractable> p_interactables)
    {
        IInteractable? bestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var interactable in p_interactables)
        {
            if (!interactable.IsInteractable) continue;

            float distance = interactable.GetDistanceTo(p_playerX, p_playerY);
            if (distance < minDistance)
            {
                minDistance = distance;
                bestTarget = interactable;
            }
        }

        return bestTarget;
    }
}
