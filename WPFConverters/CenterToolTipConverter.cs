﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ODEliteTracker.WPFConverters
{
    public class CentreToolTipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.FirstOrDefault(v => v == DependencyProperty.UnsetValue) != null)
            {
                return double.NaN;
            }
            double placementTargetWidth = (double)values[0];
            double toolTipWidth = (double)values[1];
            return (placementTargetWidth / 2.0) - (toolTipWidth / 2.0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
