using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Collections;
using OpcUaViewer.Core.Contracts;

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

// Returns true when the string is non-null and non-empty, false otherwise
[ValueConversion(typeof(string), typeof(bool))]
public class NullOrEmptyToBoolConverter : IValueConverter
{
    public static readonly NullOrEmptyToBoolConverter Default = new();

    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is string s && !string.IsNullOrEmpty(s);

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

[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => DependencyProperty.UnsetValue;
}

// Calls ISettingsPanel.CreateView() and caches the result per panel instance
public class SettingsPanelViewConverter : IValueConverter
{
    private readonly Dictionary<ISettingsPanel, FrameworkElement> _cache = [];

    public object? Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is not ISettingsPanel panel) return null;
        if (!_cache.TryGetValue(panel, out var view))
        {
            view = panel.CreateView();
            _cache[panel] = view;
        }
        return view;
    }

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => DependencyProperty.UnsetValue;
}
