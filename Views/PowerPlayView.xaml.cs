using System.Windows.Controls;

namespace ODEliteTracker.Views
{
    /// <summary>
    /// Interaction logic for PowerPlayView.xaml
    /// </summary>
    public partial class PowerPlayView : UserControl
    {
        public PowerPlayView()
        {
            InitializeComponent();
        }

        private void ThisCycleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox box && box.SelectedItem != null)
            {
                box.ScrollIntoView(box.SelectedItem);
            }
        }
    }
}
