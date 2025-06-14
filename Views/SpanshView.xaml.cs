using ODEliteTracker.Models;
using ODEliteTracker.ViewModels;
using ODEliteTracker.ViewModels.ModelViews.Spansh;
using ODEliteTracker.Views.Dialogs;
using ODMVVM.Services.MessageBox;
using ODMVVM.Views.MessageBox;
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
                spanshViewModel.OnErrorProcessingCSV += SpanshViewModel_OnErrorProcessingCSV;
            }
        }

        private void SpanshView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshViewModel spanshViewModel)
            {
                spanshViewModel.OnErrorProcessingCSV -= SpanshViewModel_OnErrorProcessingCSV;
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

                if (selector.Result > CsvType.None)
                {
                    model.ForceParseCSV(e.Filename, selector.Result);
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
    }
}
