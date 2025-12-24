using ODEliteTracker.Notifications.Themes;
using ToastNotifications.Position;

namespace ODEliteTracker.Models.Settings
{
    public enum NotificationSize
    {
        Small,
        Medium,
        Large,
    }

    public enum NotificationPlacement
    {
        Monitor,
        Application
    }

    [Flags]
    public enum NotificationOptions
    {
        None,
        System = 1 << 0,
        Station = 1 << 1,
        ShipScanned = 1 << 2,
        CopyToClipboard = 1 << 3,
        FleetCarrierReady = 1 << 4,
        SqaudCarrierReady = 1 << 5,
        Docking = 1 << 6,
        All = System | Station | ShipScanned | CopyToClipboard | FleetCarrierReady | SqaudCarrierReady | Docking
    }

    public sealed class NotificationSettings
    {
        public int DisplayTime { get; set; }
        public Corner DisplayRegion { get; set; } = Corner.BottomRight;
        public NotificationSize Size { get; set; } = NotificationSize.Medium;
        public NotificationPlacement Placement { get; set; } = NotificationPlacement.Monitor;
        public NotificationOptions Options { get; set; } = NotificationOptions.All;
        public int MaxNotificationCount { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
        public NotificationTheme CurrentTheme { get; set; } = NotificationTheme.Elite;

        public static NotificationSettings GetDefault()
        {
            return new()
            {
                DisplayTime = 10,
                DisplayRegion = Corner.BottomRight,
                Size = NotificationSize.Medium,
                Placement = NotificationPlacement.Monitor,
                Options = NotificationOptions.All,
                MaxNotificationCount = 8,
                XOffset = 40,
                YOffset = 20,
                NotificationsEnabled = true,
                CurrentTheme = NotificationTheme.Elite
            };
        }

        public NotificationSettings Clone()
        {
            return new()
            {
                DisplayTime = this.DisplayTime,
                DisplayRegion = this.DisplayRegion,
                Size = this.Size,
                MaxNotificationCount = this.MaxNotificationCount,
                XOffset = this.XOffset,
                YOffset = this.YOffset,
                NotificationsEnabled = this.NotificationsEnabled,
                CurrentTheme = NotificationTheme.Elite,
                Options = this.Options
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not NotificationSettings setting)
                return false;

            return DisplayTime == setting.DisplayTime
                && DisplayRegion == setting.DisplayRegion
                && Options == setting.Options
                && Size == setting.Size
                && MaxNotificationCount == setting.MaxNotificationCount
                && XOffset == setting.XOffset
                && YOffset == setting.YOffset
                && Placement == setting.Placement
                && NotificationsEnabled == setting.NotificationsEnabled
                && CurrentTheme == setting.CurrentTheme;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
