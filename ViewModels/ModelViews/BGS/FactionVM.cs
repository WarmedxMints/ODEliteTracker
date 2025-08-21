using EliteJournalReader;
using ODEliteTracker.Models.BGS;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.BGS
{
    public sealed class FactionVM(Faction faction) : ODObservableObject
    {
        public string Name { get; } = faction.Name;
        public string FactionState { get; } = faction.FactionState.SplitCamelCase();
        public string Government { get; } = string.IsNullOrEmpty(faction.Government_Localised) ? faction.Government : faction.Government_Localised;
        public string Influence { get; } = $"{faction.Influence * 100:N2} %";
        public string Allegiance { get; } = faction.Allegiance;
        public string MyReputation { get; } = EliteHelpers.FactionReputationToString(faction.MyReputation);
        public bool SquadronFaction { get; } = faction.SquadronFaction;

        private int infPlus;
        public int InfPlus
        {
            get => infPlus;
            set
            {
                infPlus = value;
                OnPropertyChanged(nameof(InfPluses));
            }
        }
        public string InfPluses => infPlus == 0 ? string.Empty : $"{InfPlus:N0}";

        private long bounties;
        public long Bounties
        {
            get => bounties;
            set
            {
                bounties = value;
                OnPropertyChanged(nameof(BountyVouchers));
            }
        }
        public string BountyVouchers => bounties == 0 ? string.Empty : EliteHelpers.FormatNumber(bounties);

        private long bonds;
        public long Bonds
        {
            get => bonds;
            set
            {
                bonds = value;
                OnPropertyChanged(nameof(BondVouchers));
            }
        }
        public string BondVouchers => bonds == 0 ? string.Empty : EliteHelpers.FormatNumber(bonds);

        private int failed;
        public int Failed
        {
            get => failed;
            set
            {
                failed = value;
                OnPropertyChanged(nameof(MissionsFailed));
            }
        }
        public string MissionsFailed => failed == 0 ? string.Empty : $"{failed:N0}";

        private TradeTransactionVM? sales;
        public TradeTransactionVM? Sales
        {
            get => sales;
            set
            {
                sales = value;
                OnPropertyChanged(nameof(Sales));
            }
        }

        private TradeTransactionVM? purchases;
        public TradeTransactionVM? Purchases
        {
            get => purchases;
            set
            {
                purchases = value;
                OnPropertyChanged(nameof(Purchases));
            }
        }

        public int FootMurders { get; private set; }
        public int ShipMurders { get; private set; }

        public int TotalMurders => FootMurders + ShipMurders;
        public string Murders
        {
            get
            {
                return TotalMurders == 0 ? string.Empty : $"{TotalMurders:N0}";
            }
        }

        public long CartoDataValue { get; private set; }
        public string CartoData
        {
            get
            {
                return CartoDataValue == 0 ? string.Empty : $"{EliteHelpers.FormatNumber(CartoDataValue)}";
            }
        }

        public SearchAndRescueVM? SearchAndRescue { get; private set; }

        public FactionWarVM? Wars { get; set; }

        public bool HasData()
        {
            return infPlus != 0
                || bounties > 0
                || bonds > 0
                || failed > 0
                || sales != null && sales.Value != 0
                || purchases != null && purchases.Value != 0
                || FootMurders > 0
                || ShipMurders > 0
                || CartoDataValue > 0
                || SearchAndRescue != null && SearchAndRescue.Total > 0
                || Wars != null && Wars.Total > 0;
        }

        public void AddTraction(TradeTransaction transaction)
        {
            if (transaction.Type == Models.TransactionType.Sale)
            {
                Sales ??= new();
                Sales.AddTransaction(transaction);
                return;
            }

            Purchases ??= new();
            Purchases.AddTransaction(transaction);
        }

        public void AddMurder(SystemCrime crime)
        {
            FootMurders += crime.OnFootMurders;
            ShipMurders += crime.ShipMurders;
            OnPropertyChanged(nameof(Murders));
        }

        public void AddMurders(IEnumerable<SystemCrime> crimes)
        {
            FootMurders += crimes.Sum(x => x.OnFootMurders);
            ShipMurders += crimes.Sum(x => x.ShipMurders);
            OnPropertyChanged(nameof(Murders));
        }

        public void AddCartoData(IGrouping<string, ExplorationData> value)
        {
            CartoDataValue += value.Sum(x => x.Value);
            OnPropertyChanged(nameof(CartoData));
        }

        public void AddSearchAndRescue(SearchAndRescue item)
        {
            SearchAndRescue ??= new();

            SearchAndRescue.AddItem(item);
        }
    }
}
