using ExpressiveSharp;
using Nexus.Authors;

namespace Nexus.Entities.Authors;

public static class AuthorMappingExtensions
{
    [Expressive]
    public static bool HasBio(this Author author) => !string.IsNullOrWhiteSpace(author.ShortBio);
}
