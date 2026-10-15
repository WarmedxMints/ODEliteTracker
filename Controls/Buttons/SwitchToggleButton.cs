using System.Windows;
using System.Windows.Controls.Primitives;

namespace ODEliteTracker.Controls.Buttons
{
    public sealed class SwitchToggleButton : ToggleButton
    {
        public string OffText
        {
            get { return (string)GetValue(OffTextProperty); }
            set { SetValue(OffTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OffText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffTextProperty =
            DependencyProperty.Register(nameof(OffText), typeof(string), typeof(SwitchToggleButton), new PropertyMetadata("Off"));

        public string OnText
        {
            get { return (string)GetValue(OnTextProperty); }
            set { SetValue(OnTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnTextProperty =
            DependencyProperty.Register(nameof(OnText), typeof(string), typeof(SwitchToggleButton), new PropertyMetadata("On"));
    }
}
