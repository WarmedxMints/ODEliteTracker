using ODEliteTracker.Models.Spansh;
using ODEliteTracker.ViewModels;
using ODEliteTracker.ViewModels.ModelViews.Spansh;
using ODEliteTracker.Views.Dialogs;
using ODMVVM.Services.MessageBox;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ODEliteTracker.Views
{
    /// <summary>
    /// Interaction logic for SpanshView.xaml
    /// </summary>
    public partial class SpanshView : UserControl
    {
        public SpanshView()
        {
            InitializeComponent();
            Loaded += SpanshView_Loaded;
            Unloaded += SpanshView_Unloaded;
        }

        private void SpanshView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshViewModel spanshViewModel)
            {
                spanshViewModel.RouteController.OnErrorProcessingCSV += SpanshViewModel_OnErrorProcessingCSV;
            }
        }

        private void SpanshView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshViewModel spanshViewModel)
            {
                spanshViewModel.RouteController.OnErrorProcessingCSV -= SpanshViewModel_OnErrorProcessingCSV;
            }
        }

        private void ImportCSV(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshViewModel vm)
            {
                var filename = ODDialogService.OpenFileDialog("Browse csv files", "csv", "csv files (*.csv)|*.csv", 2);

                if (!string.IsNullOrEmpty(filename)) 
                    vm.RouteController.ImportCSV(filename);
            }
        }

        private void SpanshViewModel_OnErrorProcessingCSV(object? sender, SpanshCsvErrorEventArgs e)
        {
            if (sender is not SpanshViewModel model)
                return;

            var owner = Window.GetWindow(this);
            if (e.ErrorType == SpanshCSVError.Parse)
            {
                var selector = new SpanshCSVSelector()
                {
                    Owner = owner
                };
                selector.ShowDialog();

                if (selector.Result > SpanshRouteType.None)
                {
                    model.RouteController.ForceParseCSV(e.Filename, selector.Result);
                }
                return;
            }

            ODDialogService.ShowWithOwner(owner, "Unable to parse CSV", $"Error parsing {Path.GetFileName(e.Filename)}", MessageBoxButton.OK);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid grid && e.AddedItems.Count > 0)
            {
                grid.ScrollIntoView(e.AddedItems[0]);
            }
        }

        private void ShipGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem != null)
            {
                grid.ScrollIntoView(grid.SelectedItem);
            }
        }

        private void SpanshRouteListRowMouseClick(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridRow row
                && row.IsLoaded
                && row.DataContext is SpanshRouteTargetViewModel target
                && DataContext is SpanshViewModel vm)
            {
                vm.RouteController.MoveToSystem(target);
            }
        }

        private void ImportDestinationsFromFile(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshViewModel vm)
            {
                var filename = ODDialogService.OpenFileDialog("Browse text files", "txt", "text files (*.txt)|*.txt", 2);

                if (string.IsNullOrEmpty(filename))
                    return;

                var lines = File.ReadAllLines(filename);

                CheckLineCount(vm, lines);
            }
        }
        private void ImportDestinationsFromClipboard(object sender, RoutedEventArgs e)
        {
            CloseDestinaionsBtn(sender, e);
            if (DataContext is SpanshViewModel vm)
            {
                var data = Clipboard.GetText(TextDataFormat.Text);

                var lines = new List<string>();
                using StringReader sr = new(data);

                string? line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    lines.Add(line.Trim());
                }

                CheckLineCount(vm, lines);
            }
        }

        private void CheckLineCount(SpanshViewModel vm, IEnumerable<string> lines)
        {
            if (vm.ApiController.CurrentApiArgs.RouteType != Models.SpanshRouteTypeComboBox.TouristRoute)
            {
                var lineCount = lines.Count();
                var count = 0;

                foreach (var s in lines)
                {
                    if (vm.ApiController.CurrentApiArgs.AdditionalSystems.FirstOrDefault(x => string.Equals(x.SystemName, s, StringComparison.OrdinalIgnoreCase)) == null)
                        continue;
                    count++;
                }

                if (lineCount + count > 10)
                {
                    _ = ODDialogService.ShowWithOwner(Window.GetWindow(this), "Error", $"A maximum of 10 additional systems can be sent to the api{Environment.NewLine}{Environment.NewLine}Adding this would put the total at {lineCount + count}");

                    return;
                }
            }
            vm.ApiController.ImportDestinationsCommand.Execute(lines);
            return;
        }        

        private void ImportJson(object sender, RoutedEventArgs e)
        {       
            CloseDestinaionsBtn(sender,e);
            if (sender is MenuItem item 
                && item.Tag is SpanshRouteType routeType
                && DataContext is SpanshViewModel vm)
            {
                var filename = ODDialogService.OpenFileDialog("Browse json files", "json", "json files (*.json)|*.json", 2);

                if (string.IsNullOrEmpty(filename))
                    return;

                var response = vm.ApiController.ImportJsonRoute(routeType, filename);
                if (!response.Item1)
                {
                    _ = ODDialogService.ShowWithOwner(Window.GetWindow(this), "Error", $"{response.Item2}");

                }
            }
        }

        private void CloseDestinaionsBtn(object sender, RoutedEventArgs e)
        {
            DestinationsBtn.ClosePopup();
        }

        private async void DatagridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            await Task.Delay(100);
            RefreshShipGridLayout();
        }

        private void RefreshShipGridLayout()
        {
            SavedRoutesDataGrid.Columns[0].Width = 0;
            SavedRoutesDataGrid.UpdateLayout();
            SavedRoutesDataGrid.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }
    }
}
