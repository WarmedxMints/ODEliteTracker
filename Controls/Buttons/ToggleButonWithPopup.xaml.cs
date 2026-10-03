using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ODEliteTracker.Controls.Buttons
{
    /// <summary>
    /// Interaction logic for ToggleButonWithPopup.xaml
    /// </summary>
    public partial class ToggleButonWithPopup : UserControl
    {
        public ToggleButonWithPopup()
        {
            InitializeComponent();
        }

        public object ToggleButtonContent
        {
            get { return (ContentControl)GetValue(ToggleButtonContentProperty); }
            set { SetValue(ToggleButtonContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToggleButtonContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToggleButtonContentProperty =
            DependencyProperty.Register(nameof(ToggleButtonContent), typeof(object), typeof(ToggleButonWithPopup), new UIPropertyMetadata("Toggle"));

        public object PopupContent
        {
            get { return (ContentControl)GetValue(PopupContentProperty); }
            set { SetValue(PopupContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupContentProperty =
            DependencyProperty.Register(nameof(PopupContent), typeof(object), typeof(ToggleButonWithPopup), new PropertyMetadata("Popup"));

        public Style ButtonStyle
        {
            get { return (Style)GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register(nameof(ButtonStyle), typeof(Style), typeof(ToggleButonWithPopup), new FrameworkPropertyMetadata(Application.Current.Resources["EliteStyleToggleButton"]));



        public bool CloseOnClick
        {
            get { return (bool)GetValue(CloseOnClickProperty); }
            set { SetValue(CloseOnClickProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseOnClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseOnClickProperty =
            DependencyProperty.Register(nameof(CloseOnClick), typeof(bool), typeof(ToggleButonWithPopup), new PropertyMetadata(true));


        private void Popup_Closed(object sender, EventArgs e)
        {
            if (!object.Equals(this.ToggleBtn, Mouse.DirectlyOver))
            {
                this.ToggleBtn.IsChecked = false;
            }
        }

        private void Popup_Click(object sender, RoutedEventArgs e)
        {
            if(CloseOnClick)
                this.ToggleBtn.IsChecked = false;
        }

        public void ClosePopup()
        {
            this.Popup.IsOpen = false;
        }
    }
}
