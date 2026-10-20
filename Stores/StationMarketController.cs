using EliteJournalReader.Events;
using ODCapi.Models;
using ODEliteTracker.Models.Market;
using ODJournalDatabase.JournalManagement;
using System.Diagnostics;

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
            //Appears to take a bit before the game writes the json since hte Kestrel update.  Rather than write the json and then fire the event, it appears the game does it the other way round so give it some time
            await Task.Delay(400);
            var market = journalManager.GetMarketInfo();
            if (market == null)
            {
#if DEBUG
                Debug.WriteLine($"JSON Market Null");
#endif
                return null;
            }
            //Check the market ID.  Since the Kestrel MK2 update the game has a habit of firing the market event while failing to update the market.json file
            if (marketArgs.MarketID == market.MarketID && market.Timestamp >= marketArgs.Timestamp)
            {
#if DEBUG
                Debug.WriteLine($"JSON Market Matched");
#endif
                stationMarket = new(market, marketArgs);
                return stationMarket;
            }
#if DEBUG
            Debug.WriteLine($"JSON Market did not match.");
#endif
            return null;
        }

        internal async Task<StationMarket?> GetMarket(DockedEvent.DockedEventArgs docked)
        {
            //If our market isn't null and has been updated within the last minute just return the current market
            if (stationMarket != null && stationMarket.MarketID == docked.MarketID && stationMarket.TimeStamp >= DateTime.UtcNow.AddMinutes(-1))
            {
                return stationMarket;
            }
            //Give it a little bit for the capi to catch up
            await Task.Delay(400);
            var market = await journalManager.GetCapiMarketInfo();

            //We don't care about construction depots
            if (market == null || market.Services?.Colonisationcontribution == StationServiceState.Ok)
            {
#if DEBUG
                Debug.WriteLine($"CAPI Market Ignored as it appears to be a contruction depot or null ({market == null})\nDocked Id {docked.MarketID}\nCapi Id   {market?.Id ?? 0}");
#endif
                return null;
            }
            //Check we got the right market from the capi
            if (market.Id == docked.MarketID)
            {
#if DEBUG
                Debug.WriteLine($"CAPI Market Matched");
#endif
                stationMarket = new(market, docked);
                return stationMarket;
            }
#if DEBUG
            Debug.WriteLine($"CAPI MarketID doesn't match.\nDocked ID :{docked.MarketID} @ {docked.StationName}\nCAPI ID    :{market.Id} @ {market.Name}");
#endif

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
