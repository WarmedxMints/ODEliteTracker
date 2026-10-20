using ODEliteTracker.ViewModels;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODMVVM.Services.MessageBox;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            Loaded += ColonisationView_Loaded;
            Unloaded += ColonisationView_Unloaded;
        }

        private void ColonisationView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColonisationViewModel vm)
            {
                vm.RoutePlanner.DataChangedWarning -= DataChangedWarning;
            }
        }

        private void ColonisationView_Loaded(object sender, RoutedEventArgs e)
        {
            if(DataContext is ColonisationViewModel vm)
            {
                vm.RoutePlanner.DataChangedWarning += DataChangedWarning;
            }
        }

        private void DataChangedWarning(object? sender, EventArgs e)
        {
            if (DataContext is ColonisationViewModel vm)
            {
                var mb = ODDialogService.ShowWithOwner(Window.GetWindow(this), "Save Colonisation Route Changes?", "Warning : You have unsaved changes that will be lost!\nWould you like to save?", MessageBoxButton.YesNo);

                if (mb == MessageBoxResult.Yes)
                {
                    vm.RoutePlanner.SaveRouteCommand.Execute(null);
                }
            }
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
            if (sender is ListBox box && e.OriginalSource is Button btn && btn.DataContext is ConstructionDepotVM vm)
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

        private void PurchaseGrid_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColonisationViewModel model && e.OriginalSource is Button btn && btn.DataContext is MarketPurchaseVM market)
            {
                model.CopyToClipboard(market.SystemName);
            }
        }
    }
}
