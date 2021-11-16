using System;
using System.Globalization;
using System.Windows.Data;

namespace NordChecker.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(double))]
    public class TimeSpan2TotalSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            ((TimeSpan)value).TotalSeconds;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            TimeSpan.FromSeconds((double)value);
    }
}
