using NordChecker.Models.Domain;
using NordChecker.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Data;

namespace NordChecker.Converters
{
    [ValueConversion(typeof(AccountState), typeof(string))]
    public class Enum2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            ((AccountState)value).GetAttribute<DisplayAttribute>()?.Name ?? value.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
