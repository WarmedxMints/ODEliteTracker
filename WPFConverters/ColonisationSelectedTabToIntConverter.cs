using ODEliteTracker.ViewModels;
using System.Windows.Data;

namespace ODEliteTracker.WPFConverters
{
    internal class ColonisationSelectedTabToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (ColonisationSelectedTab)value;
        }
    }
}
