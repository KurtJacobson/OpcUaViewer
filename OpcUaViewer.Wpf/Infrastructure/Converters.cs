using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

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
