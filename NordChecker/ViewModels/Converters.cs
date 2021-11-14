﻿using HandyControl.Themes;
using NordChecker.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NordChecker.Models.Domain;

namespace NordChecker.ViewModels
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
