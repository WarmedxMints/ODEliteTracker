using ODEliteTracker.ViewModels;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODMVVM.Services.MessageBox;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ODEliteTracker.Controls.Colonisation
{
    /// <summary>
    /// Interaction logic for ColoniationRoutePlannerControl.xaml
    /// </summary>
    public partial class ColoniationRoutePlannerControl : UserControl
    {
        public ColoniationRoutePlannerControl()
        {
            InitializeComponent();
            Loaded += ColoniationRoutePlannerControl_Loaded;


        }

        private void ColoniationRoutePlannerControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColonisationRoutePlannerViewModel vm)
            {
                vm.DataChangedWarning += DataChangedWarning;
                vm.OnViewModelLive += OnViewModelLive;
                vm.SelectedSystemChanged += OnSelectedSystemChanged;
                ScrollSelectedIntoView(vm);
            }
        }

        private void OnSelectedSystemChanged(object? sender, ColonisationRouteSystemVM e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RouteListGrid.ScrollIntoView(e);
            });
        }

        private void OnViewModelLive(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (DataContext is ColonisationRoutePlannerViewModel vm)
                {
                    ScrollSelectedIntoView(vm);
                }
            });
        }

        private void ScrollSelectedIntoView(ColonisationRoutePlannerViewModel vm)
        {
            if (vm.SelectedSystem == null)
                return;

            RouteListGrid.ScrollIntoView(vm.SelectedSystem);
            return;
        }

        private void DataChangedWarning(object? sender, EventArgs e)
        {
            if (DataContext is ColonisationRoutePlannerViewModel vm)
            {
                var mb = ODDialogService.ShowWithOwner(Window.GetWindow(this), "Save Colonisation Route Changes?", "Warning : You have unsaved changes that will be lost!\nWould you like to save?", MessageBoxButton.YesNo);

                if (mb == MessageBoxResult.Yes)
                {
                    vm.SaveRouteCommand.Execute(null);
                }
            }
        }

        private void ImportSavedNavRoute(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var fileName = GetJsonFile();

            if (string.IsNullOrEmpty(fileName))
                return;

            var result = AskIfAddToRoute();

            if (result == MessageBoxResult.Cancel)
                return;

            if (vm.ParseEliteNavRoute(fileName, result == MessageBoxResult.Yes))
                return;
        }

        private void ImportSpanshJson(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var fileName = GetJsonFile();

            if (string.IsNullOrEmpty(fileName))
                return;

            var result = AskIfAddToRoute();

            if (result == MessageBoxResult.Cancel)
                return;

            if (vm.ParseSpanshRoute(fileName, result == MessageBoxResult.Yes))
                return;

            ODDialogService.ShowWithOwner(Window.GetWindow(this), "Error", "Unable to process JSON file");
        }


        private string? GetJsonFile()
        {
            Microsoft.Win32.OpenFileDialog ofd = new()
            {
                Filter = "Json | *.json",
                Multiselect = false,
            };

            var dialog = ofd.ShowDialog(Window.GetWindow(this));

            if (dialog is bool v && v == false || string.IsNullOrEmpty(ofd.FileName))
                return null;

            return ofd.FileName;
        }

        private void DeleteSelectedSystem(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm || vm.SelectedSystem == null)
                return;

            var shouldDelete = ODDialogService.ShowWithOwner(Window.GetWindow(this), "Delete System?", $"Delete {vm.SelectedSystem.SystemName}?", MessageBoxButton.YesNo);

            if (shouldDelete == MessageBoxResult.Yes)
            {
                vm.DeleteSelectedSystem(null);
            }
        }

        private void ClearRoute(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var clear = ODDialogService.ShowWithOwner(Window.GetWindow(this), "Clear Route?", $"Remove all systems in Route?", MessageBoxButton.YesNo);

            if (clear == MessageBoxResult.Yes)
            {
                vm.ClearRoute();
            }
        }

        private void SaveJson(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            Microsoft.Win32.SaveFileDialog sfd = new()
            {
                Filter = "Json | *.json",
                AddExtension = true,
                DefaultExt = ".json",
                CheckPathExists = true
            };

            var dialog = sfd.ShowDialog(Window.GetWindow(this));

            if (dialog is bool v && v == false || string.IsNullOrEmpty(sfd.FileName))
                return;

            if (vm.SaveJson(sfd.FileName))
                return;

            _ = ODDialogService.ShowWithOwner(Window.GetWindow(this), "Error", "Error saving Json file", MessageBoxButton.OK);
        }

        private void SaveCsv(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            Microsoft.Win32.SaveFileDialog sfd = new()
            {
                Filter = "csv | *.csv",
                AddExtension = true,
                DefaultExt = ".csv",
                CheckPathExists = true
            };

            var dialog = sfd.ShowDialog(Window.GetWindow(this));

            if (dialog is bool v && v == false || string.IsNullOrEmpty(sfd.FileName))
                return;

            if (vm.SaveCSV(sfd.FileName))
                return;

            _ = ODDialogService.ShowWithOwner(Window.GetWindow(this), "Error", "Error saving CSV file", MessageBoxButton.OK);
        }

        private void LoadFromJsonSave(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var filename = GetJsonFile();

            if (string.IsNullOrEmpty(filename))
                return;

            var result = AskIfAddToRoute();

            if (result == MessageBoxResult.Cancel)
                return;

            if (vm.LoadFromJson(filename, result == MessageBoxResult.Yes))
            {
                return;
            }
            ODDialogService.ShowWithOwner(Window.GetWindow(this), "Error", "Unable to process JSON file");
        }

        private MessageBoxResult AskIfAddToRoute()
        {
            return ODDialogService.ShowWithOwner(Window.GetWindow(this), "Route Options", "Add to Route or Replace Current?", MessageBoxButton.YesNoCancel, "_Add", "_Replace");
        }

        private void LoadGameNavRoute(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var result = AskIfAddToRoute();

            if (result == MessageBoxResult.Cancel)
                return;

            if (vm.GetEliteNavRoute(result == MessageBoxResult.Yes))
            {
                return;
            }
            ODDialogService.ShowWithOwner(Window.GetWindow(this), "Error", "Unable to process JSON file");
        }

        private async void CheckBodyCounts(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            btn.IsEnabled = false;
            await vm.CheckAllSystemCounts();

            btn.IsEnabled = true;
        }

        private void SelectStartOfRoute(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var system = vm.RouteSystems.FirstOrDefault();

            if (system == null)
                return;

            SelectSystem(system, vm);
        }

        private void SelectLastBuilt(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var system = vm.RouteSystems.LastOrDefault(x => x.Built);

            if (system == null)
                return;

            SelectSystem(system, vm);
        }

        private void SelectLastBuiltHub(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var system = vm.RouteSystems.LastOrDefault(x => x.Built && x.Hub);

            if (system == null)
                return;

            SelectSystem(system, vm);
        }

        private void SelectNextSystemWithNotes(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm || vm.SelectedSystem == null)
                return;

            var index = vm.RouteSystems.IndexOf(vm.SelectedSystem);

            if (index < 0 || index >= vm.RouteSystems.Count - 1)
                return;

            for (var i = index + 1; i < vm.RouteSystems.Count; i++)
            {
                var system = vm.RouteSystems[i];

                if (system.HasNotes)
                {
                    SelectSystem(system, vm);
                    return;
                }
            }

            for (var i = 0; i < vm.RouteSystems.Count; i++)
            {
                var system = vm.RouteSystems[i];

                if (system.HasNotes)
                {
                    SelectSystem(system, vm);
                    return;
                }
            }

        }

        private void SelectFirstNotBuilt(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var system = vm.RouteSystems.FirstOrDefault(x => x.Built == false);

            if (system == null)
                return;

            SelectSystem(system, vm);
        }

        private void SelectLastSystem(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ColonisationRoutePlannerViewModel vm)
                return;

            var system = vm.RouteSystems.LastOrDefault();

            if (system == null)
                return;

            SelectSystem(system, vm);
        }

        private void SelectSystem(ColonisationRouteSystemVM system, ColonisationRoutePlannerViewModel vm)
        {
            vm.SelectedSystem = system;
            ScrollSelectedIntoView(vm);
        }

        private async void RecheckSelectBodyCount(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || DataContext is not ColonisationRoutePlannerViewModel vm || vm.SelectedSystem == null)
                return;

            btn.IsEnabled = false;
            await vm.UpdateSystemBodyCounts(vm.SelectedSystem);
            btn.IsEnabled = true;
        }
    }
}
