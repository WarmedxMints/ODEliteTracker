using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ODEliteTracker.WPFConverters
{
    internal class IntToColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.Black);

            var positive = System.Convert.ToInt32(value) > 0;

            return positive ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
