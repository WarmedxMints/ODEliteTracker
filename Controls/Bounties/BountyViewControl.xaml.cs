using ODEliteTracker.ViewModels.ModelViews.Shared;
using System.Windows.Controls;

namespace ODEliteTracker.Controls.Bounties
{
    /// <summary>
    /// Interaction logic for BountyViewControl.xaml
    /// </summary>
    public partial class BountyViewControl : UserControl
    {
        public BountyViewControl()
        {
            InitializeComponent();
        }

        private void ClearAllBounties(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is BountyManagerVM vm)
            {
                vm.AddIgnoredBounties.Execute("All");
            }
        }
    }
}
