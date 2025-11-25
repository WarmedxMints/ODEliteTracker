using ODEliteTracker.ViewModels;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ODEliteTracker.Views
{
    /// <summary>
    /// Interaction logic for ColonisationView.xaml
    /// </summary>
    public partial class ColonisationView : UserControl
    {
        public ColonisationView()
        {
            InitializeComponent();
        }

        private void ResourceFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is ConstructionResourceVM obj)
            {
                if (obj.RemainingCount > 0)
                    e.Accepted = true;
                else
                    e.Accepted = false;
            }
            e.Accepted = false;
        }

        private void DepotList_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(sender is ListBox box && e.OriginalSource is Button btn && btn.DataContext is ConstructionDepotVM vm)
            {
                switch ((string)btn.Tag)
                {
                    case "1":
                        box.SelectedItem = vm;
                        break;
                    case "2":
                        break;
                }
            }
        }

//        private void Popup_Closed(object sender, EventArgs e)
//        {
//            if (!object.Equals(this.button, Mouse.DirectlyOver))
//            {
//                this.button.IsChecked = false;
//            }
//}

//private void Popup_Click(object sender, System.Windows.RoutedEventArgs e)
//{
//    this.button.IsChecked = false;
//}

private void PurchaseGrid_Click(object sender, RoutedEventArgs e)
        {
            if(DataContext is ColonisationViewModel model && e.OriginalSource is Button btn && btn.DataContext is MarketPurchaseVM market)
            {
                model.CopyToClipboard(market.SystemName);
            }
        }
    }
}
