﻿using ODEliteTracker.Models.Settings;
using ODEliteTracker.Notifications;
using ODEliteTracker.Notifications.ScanNotification;
using ODEliteTracker.Notifications.Test;
using ODEliteTracker.Stores;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace ODEliteTracker.Services
{
    public sealed class NotificationService
    {
        private readonly SettingsStore settingsStore;
        private Notifier notifier;
        private MessageOptions messageOptions;

        private bool NotificationsEnabled => settingsStore.NotificationSettings.NotificationsEnabled;

        public NotificationService(SettingsStore settingsStore)
        {
            this.settingsStore = settingsStore;
            var settings = settingsStore.NotificationSettings;
            IPositionProvider provider = settings.Placement == NotificationPlacement.Monitor ?
                new PrimaryScreenPositionProvider(settings.DisplayRegion, settings.XOffset, settings.YOffset) :
                new WindowPositionProvider(Application.Current.MainWindow, settings.DisplayRegion, settings.XOffset, settings.YOffset);

            notifier = new Notifier(cfg =>
            {
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(settings.DisplayTime), MaximumNotificationCount.FromCount(settings.MaxNotificationCount));
                cfg.PositionProvider = provider;
                cfg.DisplayOptions.Width = GetNotificationWidth(settings.Size);
                cfg.DisplayOptions.TopMost = true;
            });

            messageOptions = new MessageOptions()
            {
                FontSize = GetFontSize(settings.Size),
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = true,
                ShowCloseButton = false,
                CloseClickAction = OnNotificationClose,
                NotificationClickAction = OnNotificationClick,
                Tag = string.Empty,
            };
        }


        public void UpdateNotificationSettings(NotificationSettings settings)
        {
            notifier?.Dispose();

            IPositionProvider provider = settings.Placement == NotificationPlacement.Monitor ?
                new PrimaryScreenPositionProvider(settings.DisplayRegion, settings.XOffset, settings.YOffset) :
                new WindowPositionProvider(Application.Current.MainWindow, settings.DisplayRegion, settings.XOffset, settings.YOffset);

            notifier = new Notifier(cfg =>
            {
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(settings.DisplayTime), MaximumNotificationCount.FromCount(settings.MaxNotificationCount));
                cfg.PositionProvider = provider;
                cfg.DisplayOptions.Width = GetNotificationWidth(settings.Size);
                cfg.DisplayOptions.TopMost = true;
            });

            messageOptions = new MessageOptions()
            {
                FontSize = GetFontSize(settings.Size),
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = true,
                ShowCloseButton = false,
                CloseClickAction = OnNotificationClose,
                NotificationClickAction = OnNotificationClick,
                Tag = string.Empty,
            };
        }

        private void OnNotificationClick(NotificationBase notificationBase)
        {
            if (notificationBase is TestNotification)
            {
                ODMVVM.Helpers.OperatingSystem.OpenUrl("https://github.com/WarmedxMints/ODEliteTracker");
            }
            notificationBase.Close();
        }

        private void OnNotificationClose(NotificationBase notificationBase) { }

        internal void ShowTestNotification(NotificationSettings notificationSettings)
        {
            notifier.Notify(() => new TestNotification(string.Empty, messageOptions, notificationSettings));
        }


        internal void ShowBasicNotification(NotificationArgs args)
        {
            if (NotificationsEnabled == false || settingsStore.NotificationSettings.Options.HasFlag(args.Type) == false)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                notifier.Notify(() => new BasicNotification(string.Empty, messageOptions, args, settingsStore.NotificationSettings));
            });
        }

        internal void ShowShipTargetedNotification(string name, string type, TargetType targetType, int bounty, string faction, string? power)
        {
            if (NotificationsEnabled == false || settingsStore.NotificationSettings.Options.HasFlag(NotificationOptions.ShipScanned) == false)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                notifier.Notify(() => new ShipScannedNotification(name, messageOptions, settingsStore.NotificationSettings, type, targetType, bounty, faction, power));
            });
        }

        private static double GetNotificationWidth(NotificationSize size)
        {
            return size switch
            {
                NotificationSize.Medium => 500,
                NotificationSize.Large => 750,
                _ => 350
            };
        }

        private static double GetFontSize(NotificationSize size)
        {
            return size switch
            {
                NotificationSize.Medium => 20,
                NotificationSize.Large => 28,
                _ => 14
            };
        }

        internal void Dispose()
        {
            notifier.Dispose();
        }
    }
}
