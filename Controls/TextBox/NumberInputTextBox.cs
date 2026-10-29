using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.Controls.TextBox
{
    public sealed partial class NumberInputTextBox : System.Windows.Controls.TextBox
    {

        [GeneratedRegex("[^0-9.-]+")]
        private static partial Regex NumberWithDecimals();

        [GeneratedRegex("[^0-9-]+")]
        private static partial Regex NumberWithouDecimals();

        private readonly Regex _numberWithDecinmals = NumberWithDecimals();
        private readonly Regex _numberWithOutDecinmals = NumberWithouDecimals();

        public NumberInputTextBox()
        {
            PreviewTextInput += NumberInputTextBox_PreviewTextInput;
            KeyUp += NumberInputTextBox_KeyUp;
            LostFocus += NumberInputTextBox_LostFocus;
            DataObject.AddPastingHandler(this, PastingHandler);
            Loaded += NumberInputTextBox_Loaded;
        }

        public event RoutedEventHandler? ValueChanged;

        private void NumberInputTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyText(this, Value);
        }

        private void NumberInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ProcessText();
        }

        private void NumberInputTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                ProcessText();
            }
        }

        private void ProcessText()
        {
            var parent = Window.GetWindow(this) as FrameworkElement;
            if (AllowDecimals)
            {
                if (double.TryParse(Text, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, null,  out var result))
                {
                    Value = Math.Clamp(result, Minimum, Maximum);
                }
                Text = $"{Value:N0}";
                parent?.Focus();
                Keyboard.ClearFocus();
                ValueChanged?.Invoke(this, new());
                return;
            }
            if (int.TryParse(Text, NumberStyles.AllowThousands, null, out var intResult))
            {
                Value = Math.Clamp(intResult, Minimum, Maximum);
                Text = $"{Value:N0}";
                parent?.Focus();
                Keyboard.ClearFocus();
                ValueChanged?.Invoke(this, new());
            }
        }

        /// <summary>
        /// Allow decimal places (Default : True)
        /// </summary>
        public bool AllowDecimals
        {
            get { return (bool)GetValue(AllowDecimalsProperty); }
            set { SetValue(AllowDecimalsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowDecimals.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowDecimalsProperty =
            DependencyProperty.Register(nameof(AllowDecimals), typeof(bool), typeof(NumberInputTextBox), new PropertyMetadata(true));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumberInputTextBox), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumberInputTextBox), new PropertyMetadata(0d));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumberInputTextBox), new PropertyMetadata(double.MaxValue));


        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ApplyText(d, e.NewValue);
        }

        private static bool ApplyText(DependencyObject d, object value)
        {
            if (d is not NumberInputTextBox tb)
                return false;

            if (tb.AllowDecimals)
            {
                tb.Text = $"{value:N2}";
                return false;
            }
            tb.Text = $"{value:N0}";
            return true;
        }

        private bool IsTextAllowed(string text)
        {
            if (AllowDecimals)
            {
                return !_numberWithDecinmals.IsMatch(text);
            }

            return !_numberWithOutDecinmals.IsMatch(text);
        }
       
        private void NumberInputTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            //if (sender is not NumberInputTextBox box)
            //{
            //    return;
            //}
            //char ch = e.Text[0];
            //if (AllowDecimals)
            //{
            //    if (char.IsDigit(ch) || ch == '.')
            //    {
            //        //Here TextBox1.Text is name of your TextBox
            //        if (ch == '.' && (string.IsNullOrEmpty(box.Text) || box.Text.Contains('.')))
            //        {
            //            e.Handled = true;
            //            return;
            //        }
            //        e.Handled = false;
            //        return;
            //    }
            //    e.Handled = true;
            //    return;
            //}
            //e.Handled = !char.IsDigit(ch);
        }
        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not NumberInputTextBox)
            {
                e.CancelCommand();
                return;
            }
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
        
                if (!IsTextAllowed(text))
                    e.CancelCommand();
                return;
            }
            e.CancelCommand();
        }
    }
}
