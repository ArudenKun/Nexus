using System.IO;

namespace Nexus;

public static class AppPathsExtensions
{
    extension(AppPaths appPaths)
    {
        public string DatabasePath => $"Data Source={Path.Combine(appPaths.Data, "Nexus.db")}";
    }
}
