using System.Windows.Controls;

namespace ODEliteTracker.Views
{
    /// <summary>
    /// Interaction logic for BGSDataView.xaml
    /// </summary>
    public partial class BGSDataView : UserControl
    {
        public BGSDataView()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox box && box.SelectedItem != null)
            {
                box.ScrollIntoView(box.SelectedItem);
            }
        }
    }
}
