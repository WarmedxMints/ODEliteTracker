using ODEliteTracker.Models.Colonisation.Builds;
using System.Globalization;
using System.Windows.Data;

namespace ODEliteTracker.WPFConverters
{
    public sealed class AssetFilterToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AssetFilter filter && parameter is AssetFilter param)
            {
                return filter.HasFlag(param);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
