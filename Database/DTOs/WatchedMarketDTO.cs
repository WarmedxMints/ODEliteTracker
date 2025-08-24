using Microsoft.EntityFrameworkCore;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(MarketID))]
    public class WatchedMarketDTO
    {
        public WatchedMarketDTO() { }
        public WatchedMarketDTO(string name, ulong marketID, string commoditiesJson)
        {
            Name = name;
            MarketID = marketID;
            CommoditiesJson = commoditiesJson;
        }
        public string Name { get; set; } = string.Empty;
        public ulong MarketID { get; }
        public string CommoditiesJson { get; set; } = string.Empty;
    }
}
