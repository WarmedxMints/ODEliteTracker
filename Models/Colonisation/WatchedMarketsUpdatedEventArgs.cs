namespace ODEliteTracker.Models.Colonisation
{
    public enum MarketStatus
    {
        Added,
        Removed
    }
    public sealed class WatchedMarketsUpdatedEventArgs(MarketStatus status, WatchedMarket market)
    {
        public MarketStatus Status { get; } = status;
        public WatchedMarket Market { get; } = market;
    }
}
