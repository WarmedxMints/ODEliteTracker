using EliteJournalReader;
using EliteJournalReader.Events;
using ODCapi.Models;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Notifications;
using ODEliteTracker.Notifications.Docking;
using ODEliteTracker.Notifications.ScanNotification;
using ODEliteTracker.Notifications.Test;
using ODEliteTracker.Stores;
using System.Collections.Concurrent;
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

        private readonly ConcurrentDictionary<string, ShipScannedNotification> shipScanNotifications = [];
        private DockingNotification? dockingNotification;

        private bool NotificationsEnabled => settingsStore.NotificationSettings.NotificationsEnabled;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public NotificationService(SettingsStore settingsStore)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this.settingsStore = settingsStore;
            UpdateNotificationSettings(settingsStore.NotificationSettings);
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
                cfg.Dispatcher = Application.Current.Dispatcher;
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

        private void OnNotificationClose(NotificationBase notificationBase) 
        { 
            if (notificationBase is ShipScannedNotification notification)
            {
                _ = shipScanNotifications.TryRemove(notification.Message, out _);
                return;
            }

            if (notificationBase is DockingNotification)
            {
                dockingNotification = null;
            }
        }

        internal void ShowTestNotification(NotificationSettings notificationSettings)
        {
            notifier.Notify(() => new TestNotification(string.Empty, messageOptions, notificationSettings));
        }

        internal void SetClipboard(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ODMVVM.Helpers.OperatingSystem.SetStringToClipboard(value))
                {
                    ShowBasicNotification(new("Clipboard", [value, "Copied To Clipboard"], NotificationOptions.CopyToClipboard));
                }
            });
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
                if (shipScanNotifications.TryGetValue(name, out var notification))
                {
                    notification.UpdateBountyString(bounty, settingsStore.NotificationSettings.DisplayTime);
                    return;
                }

                var newNotification = new ShipScannedNotification(name, messageOptions, settingsStore.NotificationSettings, type, targetType, bounty, faction, power);
                shipScanNotifications.TryAdd(name, newNotification);
                notifier.Notify(() => newNotification);
            });
        }

        internal void ShowDockingNotification(string header, DockingDeniedReason reason)
        {
            if (NotificationsEnabled == false || settingsStore.NotificationSettings.Options.HasFlag(NotificationOptions.Docking) == false)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (dockingNotification != null)
                {
                    dockingNotification.Update(reason);
                    return;
                }

                dockingNotification = new DockingNotification(header, messageOptions, settingsStore.NotificationSettings, reason);         
                notifier.Notify(() => dockingNotification);
            });
        }

        internal void ShowCrimeNotification(CommitCrimeEvent.CommitCrimeEventArgs args)
        {
            if (NotificationsEnabled == false || settingsStore.NotificationSettings.Options.HasFlag(NotificationOptions.Crime) == false)
                return;

            var victim = string.IsNullOrEmpty(args.Victim_Localised) ? args.Victim : args.Victim_Localised;
            var valueText = args.Bounty == null || args.Bounty == 0 ? $"{args.Fine:N0} cr fine" : $"{args.Bounty:N0} cr bounty";

            var notificationText = new List<string>()
            {
                args.Faction
            };

            switch (args.CrimeType)
            {
                case "assault":
                    notificationText.Add($"Assaulting {victim}");                    
                    break;
                case "collidedAtSpeedInNoFireZone":
                case "collidedAtSpeedInNoFireZone_hulldamage":
                    notificationText.Add("Reckless Flying");
                    break;
                case "dockingMajorBlockingLandingPad":
                case "dockingMinorBlockingLandingPad":
                    notificationText.Add("Blocking Landing Pad");
                    break;
                case "dockingMinorBlockingAirlock":
                    notificationText.Add("Blocking Air Lock");
                    break;
                case "dockingMajorTresspass":
                case "dockingMinorTresspass":
                case "onFoot_trespass":
                    notificationText.Add("Trespass");
                    break;
                case "dumpingDangerous":
                case "dumpingNearStation":
                    notificationText.Add("Dumping Cargo");
                    break;
                case "fireInNoFireZone":
                    notificationText.Add("Violating No Fire Zone");
                    break;
                case "interdiction":
                    notificationText.Add($"Interdiction of {victim}");
                    break;
                case "murder":
                case "onFoot_murder":
                    notificationText.Add($"Murder of {victim}");
                    break;
                case "onFoot_arcCutterUse":
                    notificationText.Add("Arc Cutter Use");
                    break;
                case "onFoot_carryingIllegalData":
                    notificationText.Add("Carring Illegal Data");
                    break;
                case "onFoot_carryingIllegalGoods":
                    notificationText.Add("Carring Illegal Goods");
                    break;
                case "onFoot_damagingDefences":
                    notificationText.Add("Damging Defences");
                    break;
                case "onFoot_dataTransfer":
                    notificationText.Add("Illegal Data Transfer");
                    break;
                case "onFoot_detectionOfWeapon":
                    notificationText.Add("Detection of Weapon");
                    break;
                case "onFoot_eBreachUse":
                    notificationText.Add("E-Breach Use");
                    break;
                case "onFoot_failureToSubmitToPolice":
                    notificationText.Add("Failure to submit to security");
                    break;
                case "onFoot_identityTheft":
                    notificationText.Add("Cloning Profile");
                    break;
                case "onFoot_overchargedPort":
                case "onFoot_overchargeIntent":
                    notificationText.Add("Overcharged Energy Link Use");
                    break;
                case "onFoot_propertyTheft":
                    notificationText.Add("Property Theft");
                    break;
                case "onFoot_recklessEndangerment":
                    notificationText.Add("Reckless Endangerment");
                    break;
                case "passengerWanted":
                    notificationText.Add("Carrying Wanted Passenger");
                    break;
                case "piracy":
                    notificationText.Add($"Priacy on {victim}");
                    break;
                case "recklessWeaponsDischarge":
                    notificationText.Add("Reckless Weapons Discharge");
                    break;
                case "stationTamperingMinor":
                case "stationTamperingMajor":
                    notificationText.Add($"Tampering with {victim}");
                    break;
                default:
                    notificationText.Add($"{args.CrimeType} | {victim}");
                    break;

            }

            if (args.Bounty > 0 || args.Fine > 0)
            {
                notificationText.Add(valueText);
            }

            ShowBasicNotification(new NotificationArgs("Commit Crime", [.. notificationText], NotificationOptions.Crime));
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
            notifier?.Dispose();
        }
    }
}
