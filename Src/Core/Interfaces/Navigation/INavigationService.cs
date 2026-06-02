namespace Core.Interfaces;

using System.Collections.Generic;
using Core.Domain;

public interface INavigationService
{
    IReadOnlyList<IslandDestination> GenerateDestinations(int p_count);
    bool TryNavigate(IslandDestination p_destination);
    bool CanAffordIsland(IslandDestination p_destination);
}
