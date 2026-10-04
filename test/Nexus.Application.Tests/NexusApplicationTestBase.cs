using Volo.Abp.Modularity;

namespace Nexus;

public abstract class NexusApplicationTestBase<TStartupModule> : NexusTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
