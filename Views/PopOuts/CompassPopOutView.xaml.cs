using System.Windows.Controls;
using System.Windows.Input;

namespace ODEliteTracker.Views.PopOuts
{
    /// <summary>
    /// Interaction logic for CompassPopOutView.xaml
    /// </summary>
    public partial class CompassPopOutView : UserControl
    {
        public CompassPopOutView()
        {
            InitializeComponent();
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            if (!object.Equals(this.SettingsBtn, Mouse.DirectlyOver))
            {
                this.SettingsBtn.IsChecked = false;
            }
        }
    }
}
