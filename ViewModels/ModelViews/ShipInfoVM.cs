using ODEliteTracker.Models.Ship;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews
{
    public sealed class ShipInfoVM(ShipInfo info) : ODObservableObject
    {
        private readonly ShipInfo info = info;

        public string Name => info.Name;
        public string Ident => info.Ident;
        public string ShipType => info.ShipType;
        public double JumpRangeFullTank => info.JumpRangeFullTank;
        public string JumpRangeLaden => $"{JumpRangeLadenValue:N2} ly";
        public double JumpRangeLadenValue => info.JumpRangeLaden();
        public string JumpRangeString => $"{JumpRangeValue:N2} ly";
        public double JumpRangeValue
        {
            get
            {
                var range = info.CalcJumpRangeCurrentCargo();

                return range;
            }
        }
        public bool HasScoopBool => info.HasScoop;
        public bool NeedsLargePad => info.Size == ODMVVM.Helpers.ShipSize.L;
        public string HasScoop => info.HasScoop ? string.Empty : $"No Fuel Scoop!";
        public string HasScoopYesNo => info.HasScoop ? "Yes" : "No";
        public bool HasAFMUBool => info.Modules.FirstOrDefault(x => x.Item.StartsWith("Int_Repairer", StringComparison.OrdinalIgnoreCase)) != null;
        public string HasAFMU => HasAFMUBool ? "Yes" : "No";
        public bool HasDSSBool => info.Modules.FirstOrDefault(x => x.Item.StartsWith("Int_DetailedSurfaceScanner", StringComparison.OrdinalIgnoreCase)) != null;
        public string HasDSS => HasDSSBool ? "Yes" : "No";
        public int CargoCapacity { get; } = info.CargoCapacity;
        public double CurrentCargo => info.CurrentCargoCount;
        public double TotalRange => info.CalcTotalRange();
        public string TotalRangeText => $"{TotalRange:N2} ly";
        public int CargoCount
        {
            get
            {
                return ShipCargo.Sum(x => x.Count);
            }
        }

        public string CargoText
        {
            get
            {
                return $"Cargo {CargoCount} / {CargoCapacity}";
            }
        }
        public string CargoRemaining
        {
            get
            {
                return $"{CargoCapacity - CargoCount} Unused";
            }
        }
        public string CargoRemainingValue
        {
            get
            {
                return $"Cargo Space {CargoCapacity - CargoCount}";
            }
        }
        public ObservableCollection<ShipCargoVM> ShipCargo { get; set; } = [];
        public int ShipId => info.ShipId;
        public ShipInfo ShipInfo => info;

        internal void OnCargoUpdated(IEnumerable<ShipCargo>? e, string sorting = "")
        {
            ShipCargo.ClearCollection();

            if (e != null)
            {
                var cargo = string.IsNullOrEmpty(sorting) ?
                    e.OrderBy(x => x.Name).Select(x => new ShipCargoVM(x)) :
                    e.OrderByDescending(x => string.Equals(sorting, x.Name))
                        .ThenByDescending(x => x.Count).Select(x => new ShipCargoVM(x));

                if (cargo.Any())
                {
                    ShipCargo.AddRange(cargo);
                }
            }
            OnPropertyChanged(nameof(CargoText));
            OnPropertyChanged(nameof(CargoRemaining));
            OnPropertyChanged(nameof(CurrentCargo));
            OnPropertyChanged(nameof(CargoCapacity));
            OnPropertyChanged(nameof(CargoRemainingValue));
            OnPropertyChanged(nameof(JumpRangeValue));
            OnPropertyChanged(nameof(JumpRangeString));
        }

        internal void UpdateStats()
        {
            OnPropertyChanged(nameof(JumpRangeValue));
            OnPropertyChanged(nameof(JumpRangeString));
            OnPropertyChanged(nameof(CargoCapacity));
            OnPropertyChanged(nameof(CurrentCargo));
            OnPropertyChanged(nameof(TotalRangeText));
        }
    }
}
