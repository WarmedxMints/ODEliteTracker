using EliteJournalReader.Events;
using ODCapi.Models;
using ODEliteTracker.Models.Market;
using ODJournalDatabase.JournalManagement;

namespace ODEliteTracker.Stores
{
    public sealed class StationMarketController(IManageJournalEvents manager)
    {
        private readonly IManageJournalEvents journalManager = manager;

        private StationMarket? stationMarket;
        public StationMarket? StationMarket => stationMarket;

        internal void ClearData()
        {
            stationMarket = null;
        }

        internal async Task<StationMarket?> GetMarket(MarketEvent.MarketEventArgs marketArgs)
        {
            //If our market isn't null and has been updated within the last minute just return the current market
            if (stationMarket != null && stationMarket.MarketID == marketArgs.MarketID && stationMarket.TimeStamp >= DateTime.UtcNow.AddMinutes(-1))
            {
                return stationMarket;
            }

            await Task.Delay(400);
            var market = journalManager.GetMarketInfo();
            //Check the market ID.  Since the Kestrel MK2 update the game has a habit of firing the market event while failing to update the market.json file
            if (market != null && market.Timestamp >= marketArgs.Timestamp)
            {
                stationMarket = new(market);
                return stationMarket;                
            }

            return null;
        }

        internal async Task<StationMarket?> GetMarket(DockedEvent.DockedEventArgs docked)
        {
            //If our market isn't null and has been updated within the last minute just return the current market
            if (stationMarket != null && stationMarket.MarketID == docked.MarketID && stationMarket.TimeStamp >= DateTime.UtcNow.AddMinutes(-1))
            {
                return stationMarket;
            }

            var market = await journalManager.GetCapiMarketInfo();
        
            //We don't care about construction depots
            if (market?.Services?.Colonisationcontribution == StationServiceState.Ok)
            {
                return null;
            }
            //Check we got the right market from the capi
            if (market?.Id == docked.MarketID)
            {
                stationMarket = new(market, docked);
                return stationMarket;
            }

            return null;
        }

        internal StationMarket? GetMarket()
        {
            var market = journalManager.GetMarketInfo();
            //Check the market ID.  Since the Kestrel MK2 update the game has a habit of firing the market event while failing to update the market.json file
            if (market != null)
            {
                stationMarket = new(market);
                return stationMarket;
            }

            return null;
        }

        internal StationCommodity? GetMarketItemForSale(string fdevName, string name, long id = -1)
        {
            if (stationMarket == null)
                return null;

            var item = GetItem(stationMarket.ItemsForPurchase, fdevName, name, id);

            return item;
        }

        internal StationCommodity? GetMarketItemToBuy(string fdevName, string name, long id = -1)
        {
            if (stationMarket == null)
                return null;

            var item = GetItem(stationMarket.ItemsForSale, fdevName, name, id);

            return item;
        }

        private static StationCommodity? GetItem(IEnumerable<StationCommodity> commodites, string fdevName, string name, long id)
        {
            var item = commodites.FirstOrDefault(x => string.Equals(x.Name, fdevName));
            item ??= commodites.FirstOrDefault(x => x.Id == id);
            item ??= commodites.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (item == null)
                return null;

            return item;
        }
    }
}
