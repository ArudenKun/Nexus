using System.Collections.Generic;

namespace Nexus.Extensions;

public static class EnumerableExtensions
{
    public static List<T> AsList<T>(this IEnumerable<T> values)
    {
        if (values is List<T> list)
        {
            return list;
        }

        return [.. values];
    }

    public static T[] AsArray<T>(this IEnumerable<T> values)
    {
        if (values is T[] array)
        {
            return array;
        }

        return [.. values];
    }
}
