using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace NordChecker.Converters
{
    /// <summary>
    /// Represents a chain of <see cref="IValueConverter"/>s to be executed in succession.
    /// </summary>
    [ContentProperty(nameof(Converters))]
    [ContentWrapper(typeof(ValueConverterCollection))]
    public class ConverterChain : IValueConverter
    {
        public ValueConverterCollection Converters { get; } = new();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Converters
                .Aggregate(value, (current, converter) =>
                    converter.Convert(current, targetType, parameter, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Converters.Reverse()
                .Aggregate(value, (current, converter) =>
                    converter.Convert(current, targetType, parameter, culture));
        }
    }

    /// <summary>
    /// Represents a collection of <see cref="IValueConverter"/>s.
    /// </summary>
    public sealed class ValueConverterCollection : Collection<IValueConverter> { }
}
