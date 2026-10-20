using ODEliteTracker.ViewModels;
using ODMVVM.Navigation;
using ODMVVM.Services.MessageBox;
using ODMVVM.Views;
using System.Windows;

namespace ODEliteTracker.Views
{ 
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ODWindowBase
    {
        private bool loaded;
        public MainWindow(IODNavigationService oDNavigationService)
        {
            InitializeComponent();
            NavView.AssignNavigation(oDNavigationService);
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.JournalManager.CAPIErrorMessage += JournalManager_CAPIErrorMessage;
                WindowState = viewModel.WindowPosition.State;
                base.WindowBase_Loaded(sender, e);
                viewModel.WindowPositionReset += OnWindowPositionReset;
                await viewModel.Initialise();
            }

            loaded = true;           
        }

        private void JournalManager_CAPIErrorMessage(object? sender, string e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ODDialogService.ShowWithOwnerNonBlocking(this, "CAPI Error", e);
            });
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.OnClose();
            }
        }

        private void OnWindowPositionReset(object? sender, EventArgs e)
        {
            WindowState = System.Windows.WindowState.Normal;
        }

        protected override void StateChangeRaised(object? sender, EventArgs e)
        {
            if (loaded && DataContext is MainViewModel vm)
            {
                vm.WindowPosition.State = WindowState;
            }
            base.StateChangeRaised(sender, e);
        }
    }
}