using System;
using System.Globalization;
using System.Windows.Data;

namespace NordChecker.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class NumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var numberFormatInfo = (NumberFormatInfo)culture.NumberFormat.Clone();
            numberFormatInfo.NumberGroupSeparator = " ";
            return ((int)value).ToString("#,0", numberFormatInfo);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
