using ODEliteTracker.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace ODEliteTracker.Controls.Slilders
{
    /// <summary>
    /// Interaction logic for ExponetialSliderWithTextBox.xaml
    /// </summary>
    public partial class ExponetialSliderWithTextBox : UserControl
    {
        public ExponetialSliderWithTextBox()
        {
            InitializeComponent();
            Loaded += ExponetialSliderWithTextBox_Loaded;
        }

        private void ExponetialSliderWithTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSlider();
        }

        public double TextBoxWidth
        {
            get { return (double)GetValue(TextBoxWidthProperty); }
            set { SetValue(TextBoxWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBoxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxWidthProperty =
            DependencyProperty.Register(nameof(TextBoxWidth), typeof(double), typeof(ExponetialSliderWithTextBox), new PropertyMetadata(100d));


        public double SliderWidth
        {
            get { return (double)GetValue(SliderWidthProperty); }
            set { SetValue(SliderWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SliderWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SliderWidthProperty =
            DependencyProperty.Register(nameof(SliderWidth), typeof(double), typeof(ExponetialSliderWithTextBox), new PropertyMetadata(300d));



        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(ExponetialSliderWithTextBox), new PropertyMetadata(0d));


        public double MidPoint
        {
            get { return (double)GetValue(MidPointProperty); }
            set { SetValue(MidPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MidPointProperty =

            DependencyProperty.Register("MidPoint", typeof(double), typeof(ExponetialSliderWithTextBox), new PropertyMetadata(75d));
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(ExponetialSliderWithTextBox), new PropertyMetadata(100d));


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ExponetialSliderWithTextBox), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool AllowDecimals
        {
            get { return (bool)GetValue(AllowDecimalsProperty); }
            set { SetValue(AllowDecimalsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowDecimals.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowDecimalsProperty =
            DependencyProperty.Register(nameof(AllowDecimals), typeof(bool), typeof(ExponetialSliderWithTextBox), new PropertyMetadata(false));

        public bool IgnoreSliderChange;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IgnoreSliderChange)
                return;

            var newValue = MathsHelpers.FromLinearBezier(e.NewValue, Minimum, MidPoint, Maximum);

            if (AllowDecimals)
            {
                Value = newValue;                
                return;
            }
            Value = Convert.ToInt32(newValue);
        }

        private void TextBoxValueChanged(object sender, RoutedEventArgs e)
        {
            UpdateSlider();
        }

        private void UpdateSlider()
        {
            IgnoreSliderChange = true;
            var value = MathsHelpers.ToLinearBezier(Value, Minimum, MidPoint, Maximum);
            Slider.Value = value;
            IgnoreSliderChange = false;
        }
    }
}
