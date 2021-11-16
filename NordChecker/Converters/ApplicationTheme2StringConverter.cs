using HandyControl.Themes;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NordChecker.Converters
{
    [ValueConversion(typeof(ApplicationTheme), typeof(string))]
    public class ApplicationTheme2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ApplicationTheme)value switch
            {
                ApplicationTheme.Light => "Светлая",
                ApplicationTheme.Dark  => "Тёмная",
                _ => value.ToString()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
