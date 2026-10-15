using ODEliteTracker.Models.Spansh;
using ODEliteTracker.ViewModels.ModelViews.Spansh;
using ODEliteTracker.ViewModels.PopOuts;
using ODEliteTracker.Views.Dialogs;
using ODMVVM.Services.MessageBox;
using System.Windows;
using System.Windows.Controls;

namespace ODEliteTracker.Views.PopOuts
{
    /// <summary>
    /// Interaction logic for SpanshPopOutView.xaml
    /// </summary>
    public partial class SpanshPopOutView : UserControl
    {
        public SpanshPopOutView()
        {
            InitializeComponent();
            Loaded += SpanshPopOutView_Loaded;
            Unloaded += SpanshPopOutView_Unloaded;
        }        

        private void SpanshPopOutView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshPopOutViewModel spanshViewModel)
            {
                spanshViewModel.RouteController.OnErrorProcessingCSV += SpanshViewModel_OnErrorProcessingCSV;
            }
        }

        private void SpanshPopOutView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshPopOutViewModel spanshViewModel)
            {
                spanshViewModel.RouteController.OnErrorProcessingCSV -= SpanshViewModel_OnErrorProcessingCSV;
            }
        }

        private void ImportCSV(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshPopOutViewModel vm)
            {
                var filename = ODDialogService.OpenFileDialog("Browse csv files", "csv", "csv files (*.csv)|*.csv", 2);

                vm.RouteController.ImportCSV(filename);
            }
        }

        private void SpanshViewModel_OnErrorProcessingCSV(object? sender, SpanshCsvErrorEventArgs e)
        {
            if (sender is not SpanshPopOutViewModel model)
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

            ODDialogService.ShowWithOwner(owner, "Unable to parse CSV", $"Error parsing {System.IO.Path.GetFileName(e.Filename)}", MessageBoxButton.OK);
        }
    }
}
