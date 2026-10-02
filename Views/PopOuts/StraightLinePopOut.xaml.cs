using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ODEliteTracker.Views.PopOuts
{
    /// <summary>
    /// Interaction logic for StraightLinePopOut.xaml
    /// </summary>
    public partial class StraightLinePopOut : UserControl
    {
        public StraightLinePopOut()
        {
            InitializeComponent();
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            if (sender is Popup popup)
            {
                switch (popup.Tag)
                {
                    case "SetStarPos":
                        if (!object.Equals(this.SetStarPosBtn, Mouse.DirectlyOver))
                        {
                            this.SetStarPosBtn.IsChecked = false;
                        }
                        break;
                    case "SetTargetPos":
                        if (!object.Equals(this.SetTargetPosBtn, Mouse.DirectlyOver))
                        {
                            this.SetTargetPosBtn.IsChecked = false;
                        }
                        break;
                }
            }
        }

        private void Popup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Popup popup)
            {
                switch (popup.Tag)
                {
                    case "SetStarPos":
                        this.SetStarPosBtn.IsChecked = false;
                        break;
                    case "SetTargetPos":
                        this.SetTargetPosBtn.IsChecked = false;
                        break;
                }
            }
        }
    }
}
