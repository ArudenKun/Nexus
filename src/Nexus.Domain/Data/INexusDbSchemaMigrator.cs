using System.Threading.Tasks;

namespace Nexus.Data;

public interface INexusDbSchemaMigrator
{
    Task MigrateAsync();
}
