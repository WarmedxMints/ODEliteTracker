using EliteJournalReader.Events;
using ODEliteTracker.Models.Galaxy;

namespace ODEliteTracker.Models.BGS
{
    public sealed class TradeTransaction
    {
        public TradeTransaction(MarketBuyEvent.MarketBuyEventArgs evt, FactionData data)
        {
            TransactionTime = evt.Timestamp;
            Type = TransactionType.Purchase;
            Faction = data;
            ItemName = evt.Type;
            Value = evt.TotalCost;
            Count = evt.Count;
        }

        public TradeTransaction(MarketSellEvent.MarketSellEventArgs evt, FactionData data)
        {
            TransactionTime = evt.Timestamp;
            Type = TransactionType.Sale;
            Faction = data;
            ItemName = evt.Type;
            Value = evt.TotalSale;
            Count = evt.Count;
        }

        public DateTime TransactionTime { get; }
        public TransactionType Type { get;  }
        public FactionData Faction { get;  }
        public string ItemName { get;  }
        public long Value { get;  }
        public long Count { get; }
    }
}
