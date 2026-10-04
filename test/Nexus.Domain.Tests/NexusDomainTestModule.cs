using Volo.Abp.Modularity;

namespace Nexus;

[DependsOn(
    typeof(NexusDomainModule),
    typeof(NexusTestBaseModule)
)]
public class NexusDomainTestModule : AbpModule
{

}
