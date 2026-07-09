using ODEliteTracker.ViewModels;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ODEliteTracker.Controls.Colonisation
{
    /// <summary>
    /// Interaction logic for DepotList.xaml
    /// </summary>
    public partial class DepotList : UserControl
    {
        public DepotList()
        {
            InitializeComponent();
        }

        public IEnumerable<ConstructionDepotVM> Depots
        {
            get { return (IEnumerable<ConstructionDepotVM>)GetValue(DepotsProperty); }
            set { SetValue(DepotsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Depots.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DepotsProperty =
            DependencyProperty.Register("Depots", typeof(IEnumerable<ConstructionDepotVM>), typeof(DepotList), new PropertyMetadata(null));

        public ConstructionDepotVM SelectedDepot
        {
            get { return (ConstructionDepotVM)GetValue(SelectedDepotProperty); }
            set { SetValue(SelectedDepotProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedDepot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedDepotProperty =
            DependencyProperty.Register("SelectedDepot", typeof(ConstructionDepotVM), typeof(DepotList), new PropertyMetadata(null));

        public ICommand SelectDepotCommand
        {
            get { return (ICommand)GetValue(SelectDepotCommandProperty); }
            set { SetValue(SelectDepotCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectDepotCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectDepotCommandProperty =
            DependencyProperty.Register("SelectDepotCommand", typeof(ICommand), typeof(DepotList), new PropertyMetadata(null));

        private void SetDepotActiveState(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColonisationViewModel vm && sender is MenuItem item && item.DataContext is ConstructionDepotVM depot)
            {
                vm.SetActiveStateCommand.Execute(depot);
            }
        }

        private void AddRemoveShoppingList(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColonisationViewModel vm && sender is MenuItem item && item.DataContext is ConstructionDepotVM depot)
            {
                vm.AddShoppingListCommand.Execute(depot);
            }
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColonisationViewModel vm && sender is MenuItem item && item.DataContext is ConstructionDepotVM depot)
            {
                vm.SetClipboardCommand.Execute(depot.SystemNameText);
            }
        }
    }
}
