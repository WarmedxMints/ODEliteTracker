using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ODEliteTracker.Controls.Colonisation
{
    /// <summary>
    /// Interaction logic for AssetSelectionMenu.xaml
    /// </summary>
    public partial class AssetSelectionMenu : UserControl
    {
        public AssetSelectionMenu()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(AssetSelectionMenu));

        public ICommand Command 
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                "IsChecked",
                typeof(bool),
                typeof(AssetSelectionMenu),
                new PropertyMetadata(false));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
    }
}
