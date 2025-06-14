using ODEliteTracker.Models;
using ODMVVM.Commands;
using ODMVVM.Views;
using System.Windows.Input;

namespace ODEliteTracker.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for SpanshCSVSelector.xaml
    /// </summary>
    public partial class SpanshCSVSelector : ODWindowBase
    {
        public SpanshCSVSelector()
        {
            DataContext = this;
            InitializeComponent();

            SetAsToolWindow();

            SetCSVType = new ODRelayCommand<CsvType>(SelectCSVType);
        }

        public ICommand SetCSVType { get; }
        public CsvType Result { get; private set; } = CsvType.None;

        private void SelectCSVType(CsvType csvType)
        {
            Result = csvType;
            DialogResult = true;
        }

        private void Image_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ODMVVM.Helpers.OperatingSystem.OpenUrl("https://www.spansh.co.uk/plotter");
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
