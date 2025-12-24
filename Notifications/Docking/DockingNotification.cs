using EliteJournalReader;
using ODEliteTracker.Models.Settings;
using System.Windows;
using ToastNotifications.Core;
using Application = System.Windows.Application;

namespace ODEliteTracker.Notifications.Docking
{
    public sealed class DockingNotification : NotificationBase
    {
        public double? HeaderFontSize => Options.FontSize * 1.2;
        public Thickness TextMargin => Options.FontSize is null ? new(0, 0, 0, 2) : new(0, 0, 0, (double)Options.FontSize / 7);
        public Thickness BorderThickness { get; }
        public Style? BorderStyle { get; }

        public DockingNotification(string message, MessageOptions options, NotificationSettings settings, DockingDeniedReason deniedReason) : base(message, options)
        {
            Reason = ReasonToString(deniedReason);
            var thinBorder = 2;
            var thickBorder = 6;

            switch (settings.Size)
            {
                case NotificationSize.Small:
                    thinBorder = 1;
                    thickBorder = 3;
                    break;
                case NotificationSize.Medium:
                    thinBorder = 2;
                    thickBorder = 6;
                    break;
                case NotificationSize.Large:
                    thinBorder = 3;
                    thickBorder = 9;
                    break;
            }

            switch (settings.DisplayRegion)
            {
                case ToastNotifications.Position.Corner.TopLeft:
                case ToastNotifications.Position.Corner.BottomLeft:
                    BorderThickness = new(thickBorder, thinBorder, thinBorder, thinBorder);
                    BorderStyle = Application.Current.FindResource("NotificationBorderStyleLeft") as Style;
                    break;
                default:
                    BorderThickness = new(thinBorder, thinBorder, thickBorder, thinBorder);
                    BorderStyle = Application.Current.FindResource("NotificationBorderStyle") as Style;
                    break;
            }
        }

        private DockingNotificationPart? notificationPart;
        public override NotificationDisplayPart DisplayPart => notificationPart ??= new DockingNotificationPart(this);

        private string reason = string.Empty;
        public string Reason
        {
            get => reason;
            set {  reason = value; OnPropertyChanged(nameof(Reason)); }
        }

        private string ReasonToString(DockingDeniedReason reason)
        {
            return reason switch
            {
                DockingDeniedReason.NoSpace => "No Free Landing Pads",
                DockingDeniedReason.TooLarge => "Ship Too Large",
                DockingDeniedReason.Hostile => "Hostile Faction",
                DockingDeniedReason.Offences => "Wanted By Faction",
                DockingDeniedReason.Distance => "Too Far From Station",
                DockingDeniedReason.ActiveFighter => "Ship Launcher Fighter Deployed",
                DockingDeniedReason.NoReason => "No Reason Given",
                DockingDeniedReason.RestrictedAccess => "Insufficient Docking Access",
                DockingDeniedReason.JumpImminent => "Carrier Jump Imminent",
                _ => "Unknown",
            };
        }

        internal void Update(DockingDeniedReason reason)
        {
            Reason = ReasonToString(reason);
        }
    }
}
