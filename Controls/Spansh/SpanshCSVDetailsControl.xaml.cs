using System.Windows;
using System.Windows.Controls;

namespace ODEliteTracker.Controls.Spansh
{
    /// <summary>
    /// Interaction logic for SpanshCSVDetailsControl.xaml
    /// </summary>
    public partial class SpanshCSVDetailsControl : UserControl
    {
        public SpanshCSVDetailsControl()
        {
            InitializeComponent();
        }

        public Visibility ButtonsVisibility
        {
            get { return (Visibility)GetValue(ButtonsVisibilityProperty); }
            set { SetValue(ButtonsVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonsVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonsVisibilityProperty =
            DependencyProperty.Register("ButtonsVisibility", typeof(Visibility), typeof(SpanshCSVDetailsControl), new PropertyMetadata(Visibility.Visible));

        public Style DataGridStyle
        {
            get { return (Style)GetValue(DataGridRowStyleProperty); }
            set { SetValue(DataGridRowStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataGridRowStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataGridRowStyleProperty =
            DependencyProperty.Register("DataGridStyle", typeof(Style), typeof(SpanshCSVDetailsControl), new PropertyMetadata());



    }
}
