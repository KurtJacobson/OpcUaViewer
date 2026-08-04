using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Collections;

namespace OpcUaViewer.Wpf.Infrastructure;

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBoolConverter : IValueConverter
{
    public static readonly InverseBoolConverter Default = new();

    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is bool b ? !b : DependencyProperty.UnsetValue;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is bool b ? !b : DependencyProperty.UnsetValue;
}

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisibilityConverter : IValueConverter
{
    public static readonly BooleanToVisibilityConverter Default = new();

    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is Visibility.Visible;
}

// Shows Visible when the integer value equals ConverterParameter, Collapsed otherwise
[ValueConversion(typeof(int), typeof(Visibility))]
public class IndexToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is int idx && p is string s && int.TryParse(s, out int target))
            return idx == target ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => DependencyProperty.UnsetValue;
}

// Shows Visible when collection count is zero
[ValueConversion(typeof(int), typeof(Visibility))]
public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is int n && n == 0 ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => DependencyProperty.UnsetValue;
}
