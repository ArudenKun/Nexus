using ExpressiveSharp.Mapping;

namespace Nexus.Entities;

public static class StringMappingExtensions
{
    [ExpressiveFor(typeof(string), nameof(string.IsNullOrEmpty))]
    static bool IsNullOrEmpty(string? s) => s == null || s.Length == 0;

    [ExpressiveFor(typeof(string), nameof(string.IsNullOrWhiteSpace))]
    static bool IsNullOrWhiteSpace(string? s) => s == null || s.Trim().Length == 0;
}
