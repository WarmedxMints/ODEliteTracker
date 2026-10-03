using Microsoft.EntityFrameworkCore;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(MarketID), nameof(CommanderID))]
    public class WatchedMarketDTO
    {
        public WatchedMarketDTO() { }
        public WatchedMarketDTO(string name, ulong marketID, string commoditiesJson, int commanderId)
        {
            Name = name;
            MarketID = marketID;
            CommoditiesJson = commoditiesJson;
            CommanderID = commanderId;
        }
        public string Name { get; set; } = string.Empty;
        public ulong MarketID { get; }
        public string CommoditiesJson { get; set; } = string.Empty;
        public int CommanderID { get; set; } = -1;
    }
}
