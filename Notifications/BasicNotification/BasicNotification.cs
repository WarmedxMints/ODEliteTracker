using NetTopologySuite.Utilities;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Settings;
using ODMVVM.Commands;
using System.Windows;
using System.Windows.Input;
using ToastNotifications.Core;

namespace ODEliteTracker.Notifications
{
    public sealed class BasicNotification : NotificationBase
    {
        public BasicNotification(string message, MessageOptions options, NotificationArgs args, NotificationSettings settings) : base(message, options)
        {
            Args = args;
            ClickCommand = new ODRelayCommand(OnClick);

            switch (args.Type)
            {
                case NotificationOptions.Station:
                    if(args.Text.Length == 0)
                    {
                        ImageSource = "/Assets/Icons/Coriolis_sm.png";
                        break;
                    }
                    if (args.Text.Length > 1 && args.Text[1] == "Brewer Corporation")
                    {
                        ImageSource = "/Assets/Notifications/Stations/Construction.png";
                        break;
                    }

                    ImageSource = args.Text[0] switch
                    {
                        "Fleet Carrier" => "/Assets/Notifications/Stations/FleetCarrier.png",
                        "Coriolis" => "/Assets/Notifications/Stations/Coriolis.png",
                        "Orbis" => "/Assets/Notifications/Stations/Orbis.png",
                        "Outpost" or "Scientific Outpost" => "/Assets/Notifications/Stations/Outpost.png",
                        "Ocellus" => "/Assets/Notifications/Stations/Ocellus.png",
                        "Surface Port" or "Crater Port" => "/Assets/Notifications/Stations/surface_port.png",
                        "Odyssey Settlement" => "/Assets/Notifications/Stations/settlement_sm.png",
                        "Mega Ship" or "Civilian Mega Ship" => "/Assets/Notifications/Stations/Mega-Ship_Icon.png",
                        "Construction Depot" => "/Assets/Notifications/Stations/Construction.png",
                        "Asteroid Base" => "/assets/Notifications/Stations/Asteroid_Station_Icon.png",
                        "Dodec" => "/assets/Notifications/Stations/Dodec.png",
                        _ => "/Assets/Icons/Coriolis_sm.png",
                    };
                    break;
                default:
                case NotificationOptions.System:                    
                    ImageSource = "/Assets/Icons/orrery_map.png";
                    break;

                case NotificationOptions.CopyToClipboard:
                    ImageSource = "/Assets/Icons/Clipboard.png";
                    break;
                case NotificationOptions.FleetCarrierReady:
                    ImageSource = "/Assets/Icons/fleetcarrier.png";
                    break;
                case NotificationOptions.SqaudCarrierReady:
                    ImageSource = "/Assets/Icons/squadfleetcarrier.png";
                    break;
            }            

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

        private BasicNotificationPart? _displayPart;
        public override NotificationDisplayPart DisplayPart => _displayPart ??= new BasicNotificationPart(this);

        public ICommand ClickCommand { get; }
        public NotificationArgs Args { get; }
        public string ImageSource { get; }
        public double? HeaderFontSize => Options.FontSize * 1.2;
        public Thickness TextMargin => Options.FontSize is null ? new(0, 0, 0, 2) : new(0, 0, 0, (double)Options.FontSize / 7);
        public Thickness BorderThickness { get; }
        public Style? BorderStyle { get; }

        private void OnClick(object? obj)
        {
            Options.NotificationClickAction.Invoke(this);
        }
    }
}
