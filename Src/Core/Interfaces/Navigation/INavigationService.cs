namespace Core.Interfaces.Navigation;

using System.Collections.Generic;
using Core.Domain.Models;

public interface INavigationService
{
    IReadOnlyList<IslandDestination> GenerateDestinations(int p_count);
    bool TryNavigate(IInventoryManager p_inventoryManager, IslandDestination p_destination);
}
