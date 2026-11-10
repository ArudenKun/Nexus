using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Nexus.Converters;

/// <summary>
/// Converts a <see cref="string"/> (name or numeric text) or an <see cref="int"/> to an enum value
/// of the target type. <see cref="ConvertBack"/> converts an enum value back to its name (string),
/// or to its underlying integer if <paramref name="parameter"/> is <c>"int"</c>.
/// </summary>
public sealed class StringOrIntToEnumConverter : IValueConverter
{
    public static StringOrIntToEnumConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (!enumType.IsEnum)
            return value; // not our job; pass through

        try
        {
            return value switch
            {
                null => null,
                Enum e when enumType.IsInstanceOfType(e) => e,
                string s when !string.IsNullOrWhiteSpace(s) => Enum.Parse(
                    enumType,
                    s,
                    ignoreCase: true
                ),
                int i => Enum.ToObject(enumType, i),
                _ => Enum.Parse(enumType, value.ToString()!, ignoreCase: true),
            };
        }
        catch (Exception ex) when (ex is ArgumentException or OverflowException or FormatException)
        {
            // Fall back to the enum's default value rather than throwing into the binding engine.
            return Activator.CreateInstance(enumType);
        }
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is not Enum e)
            return null;

        var asInt = string.Equals(parameter as string, "int", StringComparison.OrdinalIgnoreCase);
        return asInt
            ? System.Convert.ChangeType(e, Enum.GetUnderlyingType(e.GetType()), culture)
            : e.ToString();
    }
}
