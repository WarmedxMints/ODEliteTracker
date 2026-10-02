using Microsoft.EntityFrameworkCore;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(MarketID), nameof(SystemAddress), nameof(StationName))]
    public record InactiveDepotsDTO(ulong MarketID, ulong SystemAddress, string StationName);
}
