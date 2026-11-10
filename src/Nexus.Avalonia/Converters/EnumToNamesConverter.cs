using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Nexus.Converters;

/// <summary>
/// Converts an <see cref="Enum"/> type or value to an <see cref="IEnumerable{T}"/> of its member names.
/// Pass a <see cref="Type"/> (enum type) or an enum instance. For <c>[Flags]</c> enums,
/// pass a value with <c>flags: true</c> style behavior is not implied — member names are always all distinct values.
/// </summary>
public sealed class EnumToNamesConverter : IValueConverter
{
    public static EnumToNamesConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enumType = value switch
        {
            Type { IsEnum: true } t => t,
            Enum e => e.GetType(),
            _ => null,
        };

        if (enumType is null)
            return Array.Empty<string>();

        // For [Flags] enums, exclude the zero/None sentinel by default.
        var isFlags = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);

        return Enum.GetNames(enumType)
            .Where(name =>
            {
                if (!isFlags)
                    return true;
                var v = (Enum)Enum.Parse(enumType, name);
                return System.Convert.ToUInt64(v) != 0UL;
            })
            .ToArray();
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
