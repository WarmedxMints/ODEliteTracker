using System.Text.RegularExpressions;
using System.Windows;

namespace ODEliteTracker.Controls.TextBox
{
    public sealed class NumberInputTextBox : System.Windows.Controls.TextBox
    {
        public NumberInputTextBox()
        {
            PreviewTextInput += NumberInputTextBox_PreviewTextInput;
            DataObject.AddPastingHandler(this, PastingHandler);
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


        private readonly Regex _numberWithDecinmals = new("[^0-9.-]+");
        private readonly Regex _numberWithOutDecinmals = new("[^0-9-]+");
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
            if (sender is not NumberInputTextBox box)
            {
                return;
            }
            char ch = e.Text[0];
            if (AllowDecimals)
            {
                if (char.IsDigit(ch) || ch == '.')
                {
                    //Here TextBox1.Text is name of your TextBox
                    if (ch == '.' && (string.IsNullOrEmpty(box.Text) || box.Text.Contains('.')))
                    {
                        e.Handled = true;
                        return;
                    }
                    e.Handled = false;
                    return;
                }
                e.Handled = true;
                return;
            }
            e.Handled = !char.IsDigit(ch);
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
