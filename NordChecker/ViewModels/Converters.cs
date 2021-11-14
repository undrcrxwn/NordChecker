using HandyControl.Themes;
using NordChecker.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NordChecker.Models.Domain;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Controls;

namespace NordChecker.ViewModels
{
    /// <summary>Represents a chain of <see cref="IValueConverter"/>s to be executed in succession.</summary>
    [ContentProperty("Converters")]
    [ContentWrapper(typeof(ValueConverterCollection))]
    public class ConverterChain : IValueConverter
    {
        private readonly ValueConverterCollection _converters = new ValueConverterCollection();

        /// <summary>Gets the converters to execute.</summary>
        public ValueConverterCollection Converters
        {
            get { return _converters; }
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Converters
                .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Converters
                .Reverse()
                .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        #endregion
    }

    /// <summary>Represents a collection of <see cref="IValueConverter"/>s.</summary>
    public sealed class ValueConverterCollection : Collection<IValueConverter> { }

    [ValueConversion(typeof(int), typeof(string))]
    public class NumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var numberFormatInfo = (NumberFormatInfo)culture.NumberFormat.Clone();
            numberFormatInfo.NumberGroupSeparator = " ";
            return ((int)value).ToString("#,0", numberFormatInfo);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    [ValueConversion(typeof(AccountState), typeof(string))]
    public class AccState2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (AccountState)value switch
            {
                AccountState.Unchecked => "🕒 В очереди",
                AccountState.Reserved  => "🕖 В обработке",
                AccountState.Invalid   => "❌ Невалидный",
                AccountState.Free      => "✔️ Бесплатный",
                AccountState.Premium   => "⭐ Премиум",
                _ => throw new InvalidOperationException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class Boolean2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture) => (bool)value ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) => throw new NotSupportedException();
    }

    [ValueConversion(typeof(bool), typeof(DataGridHeadersVisibility))]
    public class Boolean2DataGridHeadersVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture) => (bool)value ? DataGridHeadersVisibility.All : DataGridHeadersVisibility.None;

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) => throw new NotSupportedException();
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture) => !(bool)value;

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) => throw new NotSupportedException();
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class Boolean2ModeIconStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture) => (bool)value ? "👨🏻‍🔬" : "🦄";

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) => throw new NotSupportedException();
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class ApplicationTheme2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ApplicationTheme)value switch
            {
                ApplicationTheme.Light => "Светлая",
                ApplicationTheme.Dark => "Тёмная",
                _ => value.ToString()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    [ValueConversion(typeof(TimeSpan), typeof(double))]
    public class TimeSpan2TotalSecondsConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture) =>
            ((TimeSpan)value).TotalSeconds;

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture) =>
            TimeSpan.FromSeconds((double)value);
    }
}
