using ToastNotifications.Core;

namespace ODEliteTracker.Notifications.Docking
{
    /// <summary>
    /// Interaction logic for DockingNotificationPart.xaml
    /// </summary>
    public partial class DockingNotificationPart : NotificationDisplayPart
    {
        public DockingNotificationPart(DockingNotification notification)
        {
            InitializeComponent();
            Bind(notification);
            Unloaded += DockingNotificationPart_Unloaded;
        }

        private void DockingNotificationPart_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is DockingNotification docking)
            {
                docking.Close();
            }
        }
    }
}
