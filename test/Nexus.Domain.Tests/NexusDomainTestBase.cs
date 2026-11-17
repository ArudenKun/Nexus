using Volo.Abp.Modularity;

namespace Nexus;

/* Inherit from this class for your domain layer tests. */
public abstract class NexusDomainTestBase<TStartupModule> : NexusTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
