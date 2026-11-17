using Nexus.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Nexus.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(NexusEntityFrameworkCoreModule),
    typeof(NexusApplicationContractsModule)
)]
public class NexusDbMigratorModule : AbpModule
{
}
