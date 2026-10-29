
using Accessibility;
using ODEliteTracker.Extensions;
using ODEliteTracker.ViewModels;
using ODEliteTracker.ViewModels.ModelViews.Spansh;
using ODEliteTracker.ViewModels.ModelViews.Spansh.Api;
using System.Windows;
using System.Windows.Controls;

namespace ODEliteTracker.Controls.Spansh
{
    public partial class SpanshAPITemplates : ResourceDictionary
    {        
        public SpanshAPITemplates()
        {
            InitializeComponent();
        }

        private void RemoveAdditionalSystem(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ApiRouteSystemVM routeSystem)
            {
                var parent = btn.FindParentByType<ListBox>();

                if (parent != null && parent.DataContext is ApiBase vm)
                {
                    vm.AdditionalSystems.Remove(routeSystem);
                }
            }
        }

        private void DataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = false;
        }

        private void DataGridMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {            
            if (e.OriginalSource is DependencyObject obj)
            {
                var parent = obj.FindParentByType<Button>();

                if (parent != null && parent.DataContext is ApiRouteSystemVM vm)
                {
                    parent.Command.Execute(vm);
                    e.Handled = true;
                    return;
                }
            }
        }

        System.Windows.Controls.TextBox? TextBoxInUse;       

        private void TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox && textBox.DataContext is SpanshAPIController controller)
            {
                controller.AdditionalSystemAdded += OnAdditionalSystemAdded;
                TextBoxInUse = textBox;
            }
        }

        private void TextBoxUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox && textBox.DataContext is SpanshAPIController controller)
            {
                controller.AdditionalSystemAdded -= OnAdditionalSystemAdded;
            }
        }

        private void OnAdditionalSystemAdded(object? sender, AdditionalSystemAddedEventArgs e)
        {
            if (!e.Added || TextBoxInUse == null)
                return;

            TextBoxInUse.Text = string.Empty;
        }
    }
}
