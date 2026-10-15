using ODEliteTracker.Models.Spansh;
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

            SetCSVType = new ODRelayCommand<SpanshRouteType>(SelectCSVType);
        }

        public ICommand SetCSVType { get; }
        public SpanshRouteType Result { get; private set; } = SpanshRouteType.None;

        private void SelectCSVType(SpanshRouteType csvType)
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
