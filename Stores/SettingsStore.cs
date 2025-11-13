using ODEliteTracker.Models;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Notifications.Themes;
using ODEliteTracker.Themes;
using ODEliteTracker.Themes.Overlay;
using ODEliteTracker.ViewModels;
using ODEliteTracker.ViewModels.PopOuts;
using ODJournalDatabase.Database.DTOs;
using ODJournalDatabase.Database.Interfaces;
using ODMVVM.Navigation;
using ODMVVM.ViewModels;

namespace ODEliteTracker.Stores
{
    public sealed class SettingsStore
    {
        public SettingsStore(IODDatabaseProvider databaseProvider,
                             ThemeManager themeManager,
                             IODNavigationService navigationService,
                             NotificationThemeManager notificationTheme,
                             OverlayThemeManager overlayThemeManager)
        {
            this.databaseProvider = databaseProvider;
            this.themeManager = themeManager;
            this.navigationService = navigationService;
            this.notificationTheme = notificationTheme;
            this.overlayThemeManager = overlayThemeManager;
            this.navigationService.CurrentViewChanged += NavigationService_CurrentViewChanged;
        }

        private readonly IODDatabaseProvider databaseProvider;
        private readonly ThemeManager themeManager;
        private readonly IODNavigationService navigationService;
        private readonly NotificationThemeManager notificationTheme;
        private readonly OverlayThemeManager overlayThemeManager;

        public int SelectedCommanderID { get; set; } = 0;
        public Type CurrentViewModel { get; set; } = typeof(MassacreMissionsViewModel);
        public Theme CurrentTheme { get; set; } = Theme.OD;
        public JournalLogAge JournalAge { get; set; } = JournalLogAge.OneHundredEightyDays;
        public ODWindowPosition MainWindowPosition { get; set; } = new();
        public NotificationSettings NotificationSettings { get; set; } = NotificationSettings.GetDefault();
        public MassacreSettings MassacreSettings { get; set; } = MassacreSettings.GetDefault();
        public TradeSettings TradeSettings { get; set; } = TradeSettings.GetDefault();
        public StatusBarSettings StatusBarSettings { get; set; } = new();
        public DateTime JournalAgeDateTime
        {
            get
            {
                return JournalAge switch
                {
                    JournalLogAge.All => new DateTime(2014,12,16),
                    JournalLogAge.SevenDays => DateTime.UtcNow.AddDays(-7),
                    JournalLogAge.ThirtyDays => DateTime.UtcNow.AddDays(-30),
                    JournalLogAge.SixtyDays => DateTime.UtcNow.AddDays(-60),
                    JournalLogAge.OneHundredEightyDays => DateTime.UtcNow.AddDays(-180),
                    _ => DateTime.UtcNow.AddYears(-((int)JournalAge - 4)),
                };
            }
        }
        public double UiScale { get; set; } = 1;
        public BGSViewSettings BGSViewSettings { get; internal set; } = new();
        public PowerPlaySettings PowerPlaySettings { get; internal set; } = new();
        public ColonisationSettings ColonisationSettings { get; internal set; } = new();
        public CarrierSettings CarrierSettings { get; internal set; } = new();
        public OverlaySettings OverlaySettings { get; internal set; } = new();
        public Dictionary<int, List<PopOutParams>> PopOutParams { get; set; } = [];
        public SpanshCSVSettings SpanshCSVSettings { get; set; } = new();
        public FleetCarrierSettings FleetCarrierSettings { get; set; } = new();

        #region Persistance
        public void LoadSettings()
        {
            var settings = databaseProvider.GetAllSettings();

            if (settings != null && settings.Count != 0)
            {
                SelectedCommanderID = SettingsDTOHelpers.SettingsDtoToInt(settings.GetSettingDTO(nameof(SelectedCommanderID)));
                CurrentTheme = SettingsDTOHelpers.SettingDtoToEnum(settings.GetSettingDTO(nameof(CurrentTheme)), Theme.OD);
                UiScale = SettingsDTOHelpers.SettingsDtoToDouble(settings.GetSettingDTO(nameof(UiScale)), 1);
                ColonisationSettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(ColonisationSettings)), ColonisationSettings);
                CurrentViewModel = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(CurrentViewModel)), typeof(ColonisationViewModel));
                BGSViewSettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(BGSViewSettings)), new BGSViewSettings());
                PowerPlaySettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(PowerPlaySettings)), new PowerPlaySettings());
                JournalAge = SettingsDTOHelpers.SettingDtoToEnum(settings.GetSettingDTO(nameof(JournalAge)), JournalLogAge.OneHundredEightyDays);
                MainWindowPosition = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(MainWindowPosition)), MainWindowPosition);
                NotificationSettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(NotificationSettings)), NotificationSettings.GetDefault());
                CarrierSettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(CarrierSettings)), CarrierSettings.GetDefault());
                MassacreSettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(MassacreSettings)), MassacreSettings.GetDefault());
                TradeSettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(TradeSettings)), TradeSettings.GetDefault());
                PopOutParams = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(PopOutParams)), new Dictionary<int, List<PopOutParams>>());
                OverlaySettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(OverlaySettings)), OverlaySettings);
                SpanshCSVSettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(SpanshCSVSettings)), SpanshCSVSettings);
                StatusBarSettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(StatusBarSettings)), StatusBarSettings);
                FleetCarrierSettings = SettingsDTOHelpers.SettingDtoToObject(settings.GetSettingDTO(nameof(FleetCarrierSettings)), new FleetCarrierSettings());
            }

            //Apply Themes
            themeManager.SetTheme(CurrentTheme);
            notificationTheme.SetTheme(NotificationSettings.CurrentTheme);
            overlayThemeManager.SetTheme(OverlaySettings.CurrentTheme);

            if (MainWindowPosition.IsZero)
            {
                ODWindowPosition.ResetWindowPosition(MainWindowPosition);
            }
        }

        public void SaveSettings()
        {
            var settings = new List<SettingsDTO>
            {
                //Just in case someone closes the app while scanning a new directory
                SettingsDTOHelpers.IntToSettingsDTO(nameof(SelectedCommanderID), SelectedCommanderID > 0 ? SelectedCommanderID : 0),
                SettingsDTOHelpers.EnumToSettingsDto(nameof(CurrentTheme), CurrentTheme),
                SettingsDTOHelpers.DoubleToSettingsDTO(nameof(UiScale), UiScale),
                SettingsDTOHelpers.EnumToSettingsDto(nameof(JournalAge), JournalAge),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(CurrentViewModel), CurrentViewModel),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(ColonisationSettings), ColonisationSettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(BGSViewSettings), BGSViewSettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(PowerPlaySettings), PowerPlaySettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(MainWindowPosition), MainWindowPosition),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(NotificationSettings), NotificationSettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(CarrierSettings), CarrierSettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(MassacreSettings), MassacreSettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(TradeSettings), TradeSettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(PopOutParams), PopOutParams),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(OverlaySettings), OverlaySettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(SpanshCSVSettings), SpanshCSVSettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(StatusBarSettings), StatusBarSettings),
                SettingsDTOHelpers.ObjectToJsonStringDto(nameof(FleetCarrierSettings), FleetCarrierSettings),
            };

            databaseProvider.AddSettings(settings);
        }
        #endregion

        #region Popouts
        public List<PopOutParams> GetCommanderPopOutParams(int commanderId)
        {
            if (PopOutParams.TryGetValue(commanderId, out var outParams))
            {
                return outParams;
            }
            var list = new List<PopOutParams>();

            if (PopOutParams.TryAdd(commanderId, list))
            {
                return list;
            }
            return [];
        }

        public PopOutParams GetParams(PopOutViewModel popOut, int knownCount, int commanderId)
        {
            var popOutParams = GetCommanderPopOutParams(commanderId);

            var count = popOutParams.Count(x => x.Type == popOut.GetType());

            if (count == 0)
            {
                var ret = Models.Settings.PopOutParams.CreateParams(popOut, 1, true);
                popOutParams.Add(ret);
                return ret;
            }

            if (knownCount > 0)
            {
                var known = popOutParams.FirstOrDefault(x => x.Type == popOut.GetType() && x.Count == knownCount);

                if (known != null)
                {
                    return known;
                }
            }
            var haveParams = popOutParams.FirstOrDefault(x => x.Type == popOut.GetType() && x.Active == false);

            if (haveParams != null)
            {
                haveParams.Active = true;
                return haveParams;
            }

            haveParams = Models.Settings.PopOutParams.CreateParams(popOut, count + 1, true);
            popOutParams.Add(haveParams);
            PopOutParams.TryAdd(commanderId, popOutParams);
            return haveParams;
        }

        public void SaveParams(PopOutViewModel popOut, bool active, int commanderId)
        {
            var popOutParams = GetCommanderPopOutParams(commanderId);

            var known = popOutParams.FirstOrDefault(x => x.Type == popOut.GetType() && x.Count == popOut.Count);

            if (known != null)
            {
                known.UpdateParams(popOut, active);
                return;
            }

            known = Models.Settings.PopOutParams.CreateParams(popOut, popOut.Count, active);
            popOutParams.Add(known);
            PopOutParams.TryAdd(commanderId, popOutParams);
        }

        public event EventHandler? OnSystemGridSettingsUpdatedEvent;
        internal void OnSystemGridSettingsUpdated()
        {
            OnSystemGridSettingsUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        private void NavigationService_CurrentViewChanged(object? sender, ODViewModel? e)
        {
            if (e == null)
            {
                return;
            }
            if (e is not LoadingViewModel)
            {
                CurrentViewModel = e.GetType();
            }
        }
    }
}
