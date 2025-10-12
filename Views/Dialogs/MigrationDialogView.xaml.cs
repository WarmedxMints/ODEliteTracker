using ODEliteTracker.ViewModels.Dialogs;
using ODMVVM.Views;

namespace ODEliteTracker.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for MigrationDialogView.xaml
    /// </summary>
    public partial class MigrationDialogView : ODWindowBase
    {
        public MigrationDialogView()
        {
            SetAsToolWindow();
            InitializeComponent();
            Loaded += MigrationDialogView_Loaded;
        }

        private void MigrationDialogView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MigrationDialogViewModel viewModel)
            {
                viewModel.OnMigrationComplete += OnMigrationComplete;
            }
        }

        private void OnMigrationComplete(object? sender, bool e)
        {
            if (e)
            {
                DialogResult = true;
                return;
            }
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
