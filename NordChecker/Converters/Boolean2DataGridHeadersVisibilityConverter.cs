using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NordChecker.Converters
{
    [ValueConversion(typeof(bool), typeof(DataGridHeadersVisibility))]
    public class Boolean2DataGridHeadersVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            (bool)value ? DataGridHeadersVisibility.All : DataGridHeadersVisibility.None;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
