using Volo.Abp.Modularity;

namespace Nexus;

[DependsOn(
    typeof(NexusApplicationModule),
    typeof(NexusDomainTestModule)
)]
public class NexusApplicationTestModule : AbpModule
{

}
