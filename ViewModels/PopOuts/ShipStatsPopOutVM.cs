using Newtonsoft.Json.Linq;
using ODEliteTracker.Models.Ship;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public record ShipStatPopoutVMSave(IReadOnlyList<bool> Bools);
    public sealed class ShipStatsPopOutVM : PopOutViewModel
    {
        private readonly SharedDataStore sharedDataStore;

        public override string Name => "Ship Stats";

        public override bool IsLive => sharedDataStore.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/shipstatsbtnimage.png", UriKind.Relative);

        private ShipInfoVM?  currentShip;
        public ShipInfoVM? CurrentShip { get => currentShip; set { currentShip = value; OnPropertyChanged(nameof(CurrentShip)); } }

        #region Stats Display
        private bool showName = true;
        public bool ShowName { get => showName; set { showName = value; OnPropertyChanged(nameof(ShowName)); } }
        private bool showType = true;
        public bool ShowType { get => showType; set { showType = value; OnPropertyChanged(nameof(ShowType)); } }
        private bool showCurrentJumpRange = true;
        public bool ShowCurrentJumpRange { get => showCurrentJumpRange; set { showCurrentJumpRange = value; OnPropertyChanged(nameof(ShowCurrentJumpRange)); } }
        private bool showTotalRange = true;
        public bool ShowTotalRange { get => showTotalRange; set { showTotalRange = value; OnPropertyChanged(nameof(ShowTotalRange)); } }
        private bool showIdent = true;
        public bool ShowIdent { get => showIdent; set { showIdent = value; OnPropertyChanged(nameof(ShowIdent)); } }
        private bool showCargo = true;
        public bool ShowCargo { get => showCargo; set { showCargo = value; OnPropertyChanged(nameof(ShowCargo)); } }
        private bool showJumpLaden = true;
        public bool ShowJumpLaden { get => showJumpLaden; set { showJumpLaden = value; OnPropertyChanged(nameof(ShowJumpLaden)); } }
        private bool showModules = true;
        public bool ShowModules { get => showModules; set { showModules = value; OnPropertyChanged(nameof(ShowModules)); } }
        #endregion

        public ShipStatsPopOutVM(SharedDataStore sharedDataStore)
        {
            this.sharedDataStore = sharedDataStore;

            this.sharedDataStore.StoreLive += SharedDataStoreLive;
            this.sharedDataStore.ShipChangedEvent += ShipChanged;
            this.sharedDataStore.ShipStatsUpdated += ShipStatsUpdated;
            if (sharedDataStore.IsLive)
                _ = SharedDataStoreLive(null, true);
        }
        private async Task SharedDataStoreLive(object? sender, bool e)
        {
            if (e)
            {
                ShipChanged(null, sharedDataStore.CurrentShipInfo);
                OnModelLive();
            }
        }       

        private void ShipChanged(object? sender, ShipInfo? e)
        {
            if (sharedDataStore.CurrentShipInfo != null && CurrentShip?.ShipId != sharedDataStore.CurrentShipInfo.ShipId)
            {
                CurrentShip = new(sharedDataStore.CurrentShipInfo);
            }
        }

        private void ShipStatsUpdated(object? sender, ShipInfo? e)
        {
            if (CurrentShip == null)
                return;

            CurrentShip.UpdateStats();
        }

        protected override void ParamsUpdated()
        {
            var settings = AdditionalSettings?.ToObject<ShipStatPopoutVMSave>();
            
            if (settings == null || settings.Bools.Count < 8)
            {
                return;
            }

            ShowName = settings.Bools[0];
            ShowType = settings.Bools[1];
            ShowCurrentJumpRange = settings.Bools[2];
            ShowTotalRange = settings.Bools[3];
            ShowIdent = settings.Bools[4];
            ShowCargo = settings.Bools[5];
            ShowJumpLaden = settings.Bools[6];
            ShowModules = settings.Bools[7];
             
        }

        internal override JObject? GetAdditionalSettings()
        {
            var save = new ShipStatPopoutVMSave(
            [
                ShowName,
                ShowType,
                ShowCurrentJumpRange,
                ShowTotalRange,
                ShowIdent,
                ShowCargo,
                ShowJumpLaden,
                ShowModules,
            ]);
            return JObject.FromObject(save);
        }

        protected override void Dispose()
        {
            this.sharedDataStore.StoreLive -= SharedDataStoreLive;
            this.sharedDataStore.ShipChangedEvent -= ShipChanged;
            this.sharedDataStore.ShipStatsUpdated -= ShipStatsUpdated;
        }
    }
}
