using System.Buffers;

namespace Nexus.Core.Extensions;

public static class PathExtensions
{
    extension(string source)
    {
        public string SanitizePath(char replacementChar = '_')
        {
            if (string.IsNullOrEmpty(source))
                return source;

            var invalid = SearchValues.Create([.. Path.GetInvalidPathChars()]);
            // Split on separators, sanitize each segment, rejoin with the platform separator
            var segments = source.Split(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            );
            for (int i = 0; i < segments.Length; i++)
                segments[i] = SanitizeSegment(segments[i], invalid, replacementChar);
            return Path.Combine(segments);

            static string SanitizeSegment(string segment, SearchValues<char> invalid, char repl)
            {
                if (!segment.AsSpan().ContainsAny(invalid))
                    return segment;
                return string.Create(segment.Length, (segment, invalid, repl), static (span, s) =>
                {
                    var (seg, inv, r) = s;
                    for (int i = 0; i < seg.Length; i++)
                        span[i] = inv.Contains(seg[i]) ? r : seg[i];
                });
            }
        }

        public string CombinePath(params string[] parts) =>
            source.Length == 0 ? Path.Combine(parts) : Path.Combine([source, .. parts]);
    }
}
