﻿using ODEliteTracker.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ODEliteTracker.WPFConverters
{
    internal class SpanshCSVTypeToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CsvType type && parameter is CsvType param)
            {
                var visible = type == param;

                return visible ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
