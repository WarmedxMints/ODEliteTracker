
using ODEliteTracker.Extensions;
using ODEliteTracker.ViewModels.ModelViews.Spansh;
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

                if (parent != null && parent.DataContext is ApiArgsVM vm)
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
    }
}
