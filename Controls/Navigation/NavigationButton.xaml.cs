using ODMVVM.Navigation.Controls;
using System.Windows;
using System.Windows.Media;

namespace ODEliteTracker.Controls.Navigation
{
    /// <summary>
    /// Interaction logic for NavigationButton.xaml
    /// </summary>
    public partial class EliteStyleNavigationButton : ODNavigationButton
    {
        public Visibility SelectedBar
        {
            get { return (Visibility)GetValue(SelectedBarProperty); }
            set { SetValue(SelectedBarProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedBar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBarProperty =
            DependencyProperty.Register("SelectedBar", typeof(Visibility), typeof(EliteStyleNavigationButton), new PropertyMetadata(Visibility.Hidden));

        public ImageSource ButtonImage
        {
            get { return (ImageSource)GetValue(ButtonImageProperty); }
            set { SetValue(ButtonImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonImageProperty =
            DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(EliteStyleNavigationButton), new PropertyMetadata());


        public EliteStyleNavigationButton()
        {
            DataContext = this;
            InitializeComponent();
        }

        public override void Activate(bool setActive)
        {
            var vis = setActive ? Visibility.Visible : Visibility.Hidden;
            SelectedBar = vis;
        }
    }
}
