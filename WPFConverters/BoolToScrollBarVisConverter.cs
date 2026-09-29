using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ODEliteTracker.WPFConverters
{
    public sealed class BoolToScrollBarVisConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool v)
            {
                return ScrollBarVisibility.Visible;
            }

            return !v ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ScrollBarVisibility visibility)
            {
                return visibility == ScrollBarVisibility.Visible;
            }

            return false;
        }
    }
}
