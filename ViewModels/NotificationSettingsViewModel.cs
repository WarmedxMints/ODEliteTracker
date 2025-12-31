using ODEliteTracker.Models.Settings;
using ODEliteTracker.Notifications.Themes;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Notifications;
using ODMVVM.Commands;
using ODMVVM.ViewModels;
using System.Windows.Input;
using ToastNotifications.Position;

namespace ODEliteTracker.ViewModels
{
    public sealed class NotificationSettingsViewModel : ODViewModel
    {
        public override bool IsLive => true;

        public NotificationSettingsViewModel(SettingsStore settings, NotificationService notificationService, NotificationThemeManager themeManager)
        {
            this.settings = settings;
            this.notificationService = notificationService;
            this.themeManager = themeManager;
            NotificationSettings = new(settings.NotificationSettings);

            ChangeNotificationSize = new ODRelayCommand<NotificationSize>(OnChangeSize, (size) => size != NotificationSettings.Size);
            ChangeNotificationPos = new ODRelayCommand<Corner>(OnChangePos, (pos) => pos != NotificationSettings.DisplayRegion);
            ChangeNotificationPlacement = new ODRelayCommand<NotificationPlacement>(OnChangePosition);
            TestNotificationCommand = new ODRelayCommand(OnTestNotification);
            SetNotificationDefaults = new ODRelayCommand(OnSetNotificationDefaults, (_) => !NotificationSettings.Default);
            SaveSettings = new ODRelayCommand(OnSaveSettings, (_) => NotificationSettings.Modified);
            SetTheme = new ODRelayCommand<NotificationTheme>(OnSetTheme);
            SetOptions = new ODRelayCommand<NotificationOptions>(OnSetOptions);
        }

        private readonly SettingsStore settings;
        private readonly NotificationService notificationService;
        private readonly NotificationThemeManager themeManager;

        public NotificationSettingsVM NotificationSettings { get; set; }

        public ICommand ChangeNotificationSize { get; }
        public ICommand ChangeNotificationPos { get; }
        public ICommand ChangeNotificationPlacement { get; }
        public ICommand TestNotificationCommand { get; }
        public ICommand SetNotificationDefaults { get; }
        public ICommand SaveSettings { get; }
        public ICommand SetTheme { get; }
        public ICommand SetOptions { get; }

        private void OnChangeSize(NotificationSize size)
        {
            NotificationSettings.Size = size;
        }

        private void OnChangePos(Corner corner)
        {
            NotificationSettings.DisplayRegion = corner;
        }

        private void OnChangePosition(NotificationPlacement position)
        {
            NotificationSettings.Placement = position;
        }

        private void OnTestNotification(object? obj)
        {
            var settings = NotificationSettings.ToSettings();
            notificationService.UpdateNotificationSettings(settings);
            notificationService.ShowTestNotification(settings);
        }

        private void OnSetNotificationDefaults(object? obj)
        {
            NotificationSettings.SetDefault();
            themeManager.SetTheme(NotificationSettings.CurrentTheme);
        }

        private void OnSaveSettings(object? obj)
        {
            NotificationSettings.UpdateSaveSettings();
            var notificationSettings = NotificationSettings.ToSettings();
            notificationService.UpdateNotificationSettings(notificationSettings);
            settings.SaveSettings();
        }

        private void OnSetTheme(NotificationTheme theme)
        {
            themeManager.SetTheme(theme);
            NotificationSettings.CurrentTheme = theme;
        }

        private void OnSetOptions(NotificationOptions options)
        {
            if (NotificationSettings.Options.HasFlag(options))
            {
                NotificationSettings.Options &= ~options;
                return;
            }

            NotificationSettings.Options |= options;
        }

        public override void Dispose()
        {
            // If settings haven't been saved, reset back to the original settings
            if(NotificationSettings.Modified)
            {
                notificationService.UpdateNotificationSettings(settings.NotificationSettings);
                themeManager.SetTheme(settings.NotificationSettings.CurrentTheme);
            }
        }
    }
}
