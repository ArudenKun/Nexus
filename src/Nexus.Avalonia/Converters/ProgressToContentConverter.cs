using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;

namespace Nexus.Converters;

public class ProgressToContentConverter : IValueConverter
{
    public static readonly ProgressToContentConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
        {
            return new FAProgressRing
            {
                IsActive = true,
                Width = 50,
                Height = 50,
            };
        }
        return new Panel();
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }
}
