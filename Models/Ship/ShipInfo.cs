using EliteJournalReader;
using EliteJournalReader.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ODEliteTracker.Models.EDSY;
using ODMVVM.Helpers;
using ODMVVM.Helpers.ShipModules;

namespace ODEliteTracker.Models.Ship
{
                                
    public record ShipStatsInfo(double BaseMass, 
                                double Fuelmul,
                                double FuelPower,
                                double FuelReserve,
                                double MaxfuelPerJump,
                                double OptimalMass,
                                double GFSDRangeBoost,
                                string ShipBuild,
                                double SuperChargeMultipler,
                                int InjectionMultipler,
                                int TankSize);
    public sealed class ShipInfo
    {
        private ShipStatsInfo? shipStatsInfo;
        public List<EDModule> Modules { get; private set; }
        public int ShipId { get; private set; }
        public string Name { get; private set; }
        public string Ident { get; private set; }
        public int CargoCapacity { get; private set; }
        public EDModule FrameShiftDrive { get; private set; }
        public EDModule MainTank { get; private set; }
        public IEnumerable<EDModule>? AdditionalTanks { get; private set; }
        public EDModule? FSDBooster { get; private set; }
        public double UnladenMass { get; private set; }
        public double FuelReserve;
        public int InjectionMultipler = 1;
        private LoadoutEvent.LoadoutEventArgs loadout;
        private double currentFuel;
        public double CurrentFuel
        {
            get {
                if (this.currentFuel <= 0)
                {
                    return shipStatsInfo?.TankSize ?? FuelTankInfomation.GetFuelTankSize(MainTank.Item);
                }
                return currentFuel; 
            }
            set => currentFuel = value; 
        }
        private double reserveFuel;

        public double ReserveFuel
        {
            get 
            { 
                if (reserveFuel <= 0)
                {
                    return FuelReserve;
                }
                return reserveFuel; 
            }
            set => reserveFuel = value; 
        }

        public ShipInfo(LoadoutEvent.LoadoutEventArgs loadout)
        {
            this.loadout = loadout;
            Modules = [.. loadout.Modules];
            ShipId = loadout.ShipID;
            Name = string.IsNullOrEmpty(loadout.ShipName.Trim()) ? EliteHelpers.ConvertShipName(loadout.Ship) : loadout.ShipName;
            Ident = loadout.ShipIdent;
            CargoCapacity = loadout.CargoCapacity;
            FrameShiftDrive = loadout.Modules.First(x => string.Equals(x.Slot,"FrameShiftDrive", StringComparison.InvariantCultureIgnoreCase));
            MainTank = loadout.Modules.First(x => string.Equals(x.Slot, "FuelTank", StringComparison.InvariantCultureIgnoreCase));
            AdditionalTanks = loadout.Modules.Where(x => x.Slot != "FuelTank" && x.Item.StartsWith("int_fueltank", StringComparison.OrdinalIgnoreCase));
            FSDBooster = loadout.Modules.FirstOrDefault(x => x.Item.StartsWith("int_guardianfsdbooster", StringComparison.OrdinalIgnoreCase));
            UnladenMass = loadout.UnladenMass;
            FuelReserve = loadout.FuelCapacity.Reserve;
            ShipJson = (JObject)loadout.OriginalEvent.DeepClone();
            shipStatsInfo = GetStats().Item1;
        }

        public JObject ShipJson { get; set; }
        public List<ShipCargo> Cargo { get; set; } = [];
        public int CurrentCargoCount { get; set; }
        public bool OverchargedBoost => FrameShiftDrive.Item.EndsWith("overchargebooster_mkii", StringComparison.InvariantCultureIgnoreCase);
        public string CurrentJumpRangeString => CalcJumpRangeString(CurrentJumpRange);
        public double CurrentJumpRange => CalcJumpRangeSetCargo(0);
        public double CurrentBoostValue { get; set; } = 1;
        public double JumpRangeFullTank
        {
            get
            {
                if (shipStatsInfo == null)
                {
                    return 0;
                }
                return CalcJumpRange(shipStatsInfo, shipStatsInfo.FuelReserve + shipStatsInfo.TankSize, 0);
            }
        }
        public string ShipType => EliteHelpers.ConvertShipName(loadout.Ship);
        public bool HasScoop => loadout.Modules.FirstOrDefault(x => x.Item.StartsWith("Int_FuelScoop", StringComparison.InvariantCultureIgnoreCase)) != null;
             
        internal void Update(LoadoutEvent.LoadoutEventArgs loadout)
        {
            this.loadout = loadout;
            Modules = [.. loadout.Modules];
            ShipId = loadout.ShipID;
            Name = string.IsNullOrEmpty(loadout.ShipName.Trim()) ? EliteHelpers.ConvertShipName(loadout.Ship) : loadout.ShipName;
            Ident = loadout.ShipIdent;
            CargoCapacity = loadout.CargoCapacity;
            FrameShiftDrive = loadout.Modules.First(x => string.Equals(x.Slot, "FrameShiftDrive", StringComparison.InvariantCultureIgnoreCase));
            MainTank = loadout.Modules.First(x => string.Equals(x.Slot, "FuelTank", StringComparison.InvariantCultureIgnoreCase));
            AdditionalTanks = loadout.Modules.Where(x => x.Slot != "FuelTank" && x.Item.StartsWith("int_fueltank", StringComparison.OrdinalIgnoreCase));
            FSDBooster = loadout.Modules.FirstOrDefault(x => x.Item.StartsWith("int_guardianfsdbooster", StringComparison.OrdinalIgnoreCase));
            UnladenMass = loadout.UnladenMass;
            FuelReserve = loadout.FuelCapacity.Reserve;
            ShipJson = (JObject)loadout.OriginalEvent.DeepClone();
            shipStatsInfo = GetStats().Item1;
        }


        internal void OpenShipyard(ShipyardWebSite shipyardPreference)
        {
            string uri = string.Empty;
            switch (shipyardPreference)
            {
                case ShipyardWebSite.EDSY:
                    uri = "https://edsy.org/" + "#/I=" + ShipJson.ToString().Utf8GZipBase64UriEscape();
                    break;
                case ShipyardWebSite.Coriolis:
                    uri = $"https://coriolis.io/import?data={ShipJson.ToString().GZipBase64EscapeUri()}&bn={Uri.EscapeDataString(Name)}";
                    break;
            }

            if (string.IsNullOrEmpty(uri))
                return;

            ODMVVM.Helpers.OperatingSystem.OpenUrl(uri);
        }

        internal string GetSelf()
        {
            var ignoreNull = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            };
            var self = new EdsySelf();

            var data = ShipJson.ToObject<Data>();

            self.Data = data;

            var ret = JsonConvert.SerializeObject(self, ignoreNull);

            return ret ?? string.Empty;
        }

        internal double CalcJumpRangeSetCargo(int cargo)
        {
            if (shipStatsInfo == null)
            {
                shipStatsInfo = GetStats().Item1;
            }

            if (shipStatsInfo == null)
                return 0d;
            var fuel = ReserveFuel + CurrentFuel;
            return CalcJumpRange(shipStatsInfo, fuel, cargo);
        }

        internal double JumpRangeLaden()
        {
            if (shipStatsInfo == null)
            {
                shipStatsInfo = GetStats().Item1;
            }

            if (shipStatsInfo == null)
                return 0d;

            return CalcJumpRange(shipStatsInfo, shipStatsInfo.TankSize, CargoCapacity);
        }
        internal double CalcJumpRangeCurrentCargo()
        {
            if (shipStatsInfo == null)
            {
                shipStatsInfo = GetStats().Item1;
            }

            if (shipStatsInfo == null)
                return 0d;
            var fuel = CurrentFuel + FuelReserve;
            return CalcJumpRange(shipStatsInfo, fuel, CurrentCargoCount) * CurrentBoostValue;
        }

        internal static string CalcJumpRangeString(double jumpRange) => $"{jumpRange:N2} ly";

        internal (ShipStatsInfo?, string) GetStats()
        {
            //See if we have data for the FSD
            var fsdData = FrameShiftDriveIno.GetFsd(FrameShiftDrive.Item);

            if (fsdData == null)
                return (null, "Data for FSD missing");

            var optimalMass = GetOptimalMass(fsdData);
            var mk2Fsd = OverchargedBoost;
            var superchargeMultipler = mk2Fsd ? 6 : 4;
            InjectionMultipler = mk2Fsd ? 2 : 1;
            var rangeBoost = 0d;

            if (FSDBooster != null)
            {
                rangeBoost = GuardianFSDBoosterInfo.GetBoostValue(FSDBooster.Item);
            }
            var baseMass = (UnladenMass + FuelReserve);

            var totalFuelCapacity = FuelTankInfomation.GetFuelTankSize(MainTank.Item);
            totalFuelCapacity += AdditionalTanks?.Sum(x => FuelTankInfomation.GetFuelTankSize(x.Item)) ?? 0;

            var stats = new ShipStatsInfo(baseMass,
                                           fsdData.FuelMul,
                                           fsdData.FuelPower,
                                           FuelReserve,
                                           fsdData.MaxFuelPerJump,
                                           optimalMass,
                                           rangeBoost,
                                           GetSelf(),
                                           superchargeMultipler,
                                           InjectionMultipler,
                                           totalFuelCapacity);
            return (stats, "Ok");
        }

        public double CalcTotalRange()
        {
            if (shipStatsInfo == null)
                return 0;

            double fuelRemaining = shipStatsInfo.TankSize + shipStatsInfo.FuelReserve;
            double totalRange = 0d;
            //Add boosted range
            if (CurrentBoostValue > 1)
            {
                var fuelForJump = Math.Min(fuelRemaining, shipStatsInfo.MaxfuelPerJump);
                totalRange += CalcJumpRange(shipStatsInfo, fuelRemaining, CurrentCargoCount) * CurrentBoostValue;
                fuelRemaining -= fuelForJump;
            }
            while (fuelRemaining > 0)
            {
                var fuelForJump = Math.Min(fuelRemaining, shipStatsInfo.MaxfuelPerJump);                
                totalRange += CalcJumpRange(shipStatsInfo, fuelRemaining, CurrentCargoCount);
                fuelRemaining -= fuelForJump;
            }
            return totalRange;
        }

        private double CalcJumpRange(ShipStatsInfo info, double fuel, int cargo)
        {         
            double totalMass = UnladenMass + fuel + cargo;
            double fuelForJump = Math.Min(fuel, info.MaxfuelPerJump);

            var range =  (Math.Pow(fuelForJump / info.Fuelmul, 1.0d / info.FuelPower) * info.OptimalMass / totalMass) + info.GFSDRangeBoost;

            return range; 
        }
        private double GetOptimalMass(ModuleInfo fsdData)
        {
            if (FrameShiftDrive?.Engineering?.Modifiers == null)
                return fsdData.OptMass;

            foreach (var modification in FrameShiftDrive.Engineering.Modifiers)
            {
                if (modification.Label == "FSDOptimalMass")
                {
                    return modification.Value;
                }
            }

            return fsdData.OptMass;
        }

        internal bool UpdateFromStatus(StatusFileEvent status)
        {
            var ret = false;
            if (CurrentFuel != status.Fuel.FuelMain)
            {
                CurrentFuel = status.Fuel.FuelMain;
                ret = true;
            }

            if (ReserveFuel != status.Fuel.FuelReservoir)
            {
                ret = true;
                ReserveFuel = status.Fuel.FuelReservoir;
            }

            if (CurrentCargoCount != status.Cargo)
            {
                ret = true;
                CurrentCargoCount = status.Cargo;
            }

            return ret;
        }

        internal void UpdateCurrentBoost(JetConeBoostEvent.JetConeBoostEventArgs jetConeBoost)
        {
            CurrentBoostValue = jetConeBoost.BoostValue;
        }

        internal bool ProcessSynth(SynthesisEvent.SynthesisEventArgs synthesisEventArgs)
        {
            if (!synthesisEventArgs.Name.StartsWith("FSD", StringComparison.InvariantCultureIgnoreCase))
                return false;
            if (string.Equals("FSD Basic", synthesisEventArgs.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                CurrentBoostValue = 1.25 * InjectionMultipler;
                return true;
            }
            if (string.Equals("FSD Standard", synthesisEventArgs.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                CurrentBoostValue = 1.5 * InjectionMultipler;
                return true;
            }
            if (string.Equals("FSD Premium", synthesisEventArgs.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                CurrentBoostValue = 2 * InjectionMultipler;
                return true;
            }
            return false;
        }
    }
}
