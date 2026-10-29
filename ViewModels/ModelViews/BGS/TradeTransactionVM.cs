using ODEliteTracker.Models;
using ODEliteTracker.Models.BGS;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.BGS
{
    public sealed class TradeTransactionVM : ODObservableObject
    {
        public TransactionType Type { get; private set; }    
        public long Value { get; private set; }
        public long Count { get; private set; }

        public void AddTransaction(TradeTransaction transaction)
        {
            Type = transaction.Type;
            Value += transaction.Value;
            Count += transaction.Count;
        }

        public override string ToString()
        {
            return Value == 0 ? string.Empty : $"{EliteHelpers.FormatNumber(Value)} ({Count:N0} t)";
        }
    }
}
