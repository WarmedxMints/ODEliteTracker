using ODEliteTracker.Models.Settings;
using ODMVVM.Commands;
using System.Windows.Input;
using System.Windows;
using ToastNotifications.Core;
using System.ComponentModel;
using ODMVVM.Helpers;

namespace ODEliteTracker.Notifications.ScanNotification
{
    [Flags]
    public enum TargetType
    {
        None = 0,
        [Description("WANTED")]
        Wanted = 1 << 0,
        [Description("ENEMY")]
        Enemy = 1 << 1,
        [Description("WANTED | ENEMY")]
        WantedEnemy = Wanted | Enemy,        
    }

    public sealed class ShipScannedNotification : NotificationBase
    {
        public ShipScannedNotification(string message,
                                       MessageOptions options,
                                       NotificationSettings settings,
                                       string shipType,
                                       TargetType targetType,
                                       int bountyValue,
                                       string faction,
                                       string? power) : base(message, options)
        {
            ClickCommand = new ODRelayCommand(OnClick);

            ImageSource = "/Assets/Icons/assassin-large.png";

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

            ShipType = shipType;
            Faction = faction;
            TargetType = targetType.GetEnumDescription();
            this.bountyValue = bountyValue;
            BountyVis = bountyValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            BountyString = $"{bountyValue:N0} cr";
            PowerVis = string.IsNullOrEmpty(power) ? Visibility.Collapsed : Visibility.Visible;
            PowerString = power;
        }

        private ShipScannedNotificationPart? _displayPart;
        public override NotificationDisplayPart DisplayPart => _displayPart ??= new ShipScannedNotificationPart(this);

        public ICommand ClickCommand { get; }
        public string ImageSource { get; }
        public double? HeaderFontSize => Options.FontSize * 1.2;
        public Thickness TextMargin => Options.FontSize is null ? new(0, 0, 0, 2) : new(0, 0, 0, (double)Options.FontSize / 7);
        public Thickness BorderThickness { get; }
        public Style? BorderStyle { get; }
        public string ShipType { get; }
        public string Faction { get; }
        public string TargetType { get; }
        public Visibility BountyVis { get; }
        public string BountyString { get; private set; }
        public Visibility PowerVis { get; }
        public string? PowerString { get; }
        private int bountyValue;
        public void UpdateBountyString(int value, int displayTime)
        {
            if (bountyValue == value)
                return;

            bountyValue = value;
            BountyString = $"{value:N0} cr";
            Task.Factory.StartNew(async () =>
            {
                CanClose = false;
                await Task.Delay(displayTime * 1000);
                CanClose = true;
            });
            OnPropertyChanged(nameof(BountyString));
        }

        private void OnClick(object? obj)
        {
            Options.NotificationClickAction.Invoke(this);
        }
    }
}
