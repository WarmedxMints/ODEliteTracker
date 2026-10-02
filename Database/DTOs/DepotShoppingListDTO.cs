using Microsoft.EntityFrameworkCore;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(MarketID), nameof(SystemAddress), nameof(StationName))]
    public record DepotShoppingListDTO(ulong MarketID, ulong SystemAddress, string StationName);
}
