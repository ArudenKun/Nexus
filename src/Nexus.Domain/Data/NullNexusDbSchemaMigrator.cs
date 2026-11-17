using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Nexus.Data;

/* This is used if database provider does't define
 * INexusDbSchemaMigrator implementation.
 */
public class NullNexusDbSchemaMigrator : INexusDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
