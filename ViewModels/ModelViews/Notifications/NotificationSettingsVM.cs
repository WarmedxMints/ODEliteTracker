﻿using ODEliteTracker.Models.Settings;
using ODEliteTracker.Notifications.Themes;
using ODMVVM.ViewModels;
using ToastNotifications.Position;

namespace ODEliteTracker.ViewModels.ModelViews.Notifications
{
    public class NotificationSettingsVM(NotificationSettings settings) : ODObservableObject
    {
        private readonly NotificationSettings settings = settings;
         
        private int displayTime = settings.DisplayTime;
        public int DisplayTime 
        { 
            get => displayTime;
            set
            {
                displayTime = value;
                OnPropertyChanged(nameof(DisplayTime));
                OnPropertyChanged(nameof(Modified));
            }
        }

        private Corner displayRegion = settings.DisplayRegion;
        public Corner DisplayRegion 
        { 
            get => displayRegion;
            set
            {
                displayRegion = value;
                OnPropertyChanged(nameof(DisplayRegion));
                OnPropertyChanged(nameof(Modified));
            }
        }

        private NotificationSize size = settings.Size;
        public NotificationSize Size 
        { 
            get => size;
            set
            {
                size = value;
                OnPropertyChanged(nameof(Size));
                OnPropertyChanged(nameof(Modified));
            }
        }

        private NotificationPlacement placement = settings.Placement;
        public NotificationPlacement Placement
        {
            get => placement;
            set
            {
                placement = value;
                OnPropertyChanged(nameof(Placement));
                OnPropertyChanged(nameof(Modified));
            }
        }

        private NotificationOptions options = settings.Options;
        public NotificationOptions Options
        {
            get => options;
            set
            {
                options = value;
                OnPropertyChanged(nameof(Options));
                OnPropertyChanged(nameof(Modified));
            }
        }

        private int maxNotificationCount = settings.MaxNotificationCount;
        public int MaxNotificationCount 
        { 
            get => maxNotificationCount;
            set
            {
                maxNotificationCount = value;
                OnPropertyChanged(nameof(MaxNotificationCount));
                OnPropertyChanged(nameof(Modified));
            }
        }

        private int xOffset = settings.XOffset;
        public int XOffset
        {
            get => xOffset;
            set
            {
                xOffset = value;
                OnPropertyChanged(nameof(XOffset));
                OnPropertyChanged(nameof(Modified));
            }
        }

        private int yOffset = settings.YOffset;
        public int YOffset
        {
            get => yOffset;
            set
            {
                yOffset = value;
                OnPropertyChanged(nameof(YOffset));
                OnPropertyChanged(nameof(Modified));
            }
        }

        private bool notificationsEnabled = settings.NotificationsEnabled;
        public bool NotificationsEnabled
        {
            get => notificationsEnabled;
            set
            {
                notificationsEnabled = value;
                OnPropertyChanged(nameof(NotificationsEnabled));
                OnPropertyChanged(nameof(Modified));
            }
        }

        private NotificationTheme currentTheme = settings.CurrentTheme;
        public NotificationTheme CurrentTheme
        {
            get => currentTheme;
            set
            {
                currentTheme = value;
                OnPropertyChanged(nameof(CurrentTheme));
                OnPropertyChanged(nameof(Modified));
            }
        }
            
        public bool Modified
        {
            get
            {
                return DisplayTime != settings.DisplayTime
                    || DisplayRegion != settings.DisplayRegion
                    || Options != settings.Options
                    || Size != settings.Size
                    || MaxNotificationCount != settings.MaxNotificationCount
                    || XOffset != settings.XOffset
                    || YOffset != settings.YOffset
                    || Placement != settings.Placement
                    || NotificationsEnabled != settings.NotificationsEnabled
                    || CurrentTheme != settings.CurrentTheme;
            }
        }

        public NotificationSettings ToSettings()
        {
            return new()
            {
                DisplayTime = displayTime,
                DisplayRegion = displayRegion,
                Size = size,
                MaxNotificationCount = maxNotificationCount,
                XOffset = xOffset,
                YOffset = yOffset,
                Placement = placement,
                NotificationsEnabled = notificationsEnabled,
                Options = options
            };
        }

        public void UpdateSaveSettings()
        {
            settings.DisplayTime = displayTime;
            settings.DisplayRegion = displayRegion;
            settings.Options = options;
            settings.Size = size;
            settings.MaxNotificationCount = maxNotificationCount;
            settings.XOffset = xOffset;
            settings.YOffset = yOffset;
            settings.Placement = placement;
            settings.NotificationsEnabled = notificationsEnabled;
            settings.CurrentTheme = currentTheme;
            OnPropertyChanged(nameof(Modified));
        }

        internal void SetDefault()
        {
            var defaults = NotificationSettings.GetDefault();

            DisplayTime = defaults.DisplayTime;
            DisplayRegion = defaults.DisplayRegion;
            Size = defaults.Size;
            MaxNotificationCount = defaults.MaxNotificationCount;
            XOffset = defaults.XOffset;
            YOffset = defaults.YOffset;
            Placement = defaults.Placement;
            NotificationsEnabled = defaults.NotificationsEnabled;
            CurrentTheme = defaults.CurrentTheme;
            Options = defaults.Options;
        }
    }
}
