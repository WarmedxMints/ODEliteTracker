using ODEliteTracker.Models.Galaxy;
using System.Windows;
using System.Windows.Controls;

namespace ODEliteTracker.Controls.Galaxy
{
    /// <summary>
    /// Interaction logic for PositionControl.xaml
    /// </summary>
    public partial class PositionControl : UserControl
    {
        public Position Position
        {
            get { return (Position)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(Position), typeof(PositionControl), new PropertyMetadata(null));

        public bool InOverlay
        {
            get { return (bool)GetValue(InOverlayProperty); }
            set { SetValue(InOverlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InOverlay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InOverlayProperty =
            DependencyProperty.Register(nameof(InOverlay), typeof(bool), typeof(PositionControl), new PropertyMetadata(false));


        public PositionControl()
        {
            InitializeComponent();
        }
    }
}
