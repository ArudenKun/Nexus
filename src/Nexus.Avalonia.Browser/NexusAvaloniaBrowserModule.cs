using Volo.Abp.Modularity;

namespace Nexus;

[DependsOn(typeof(NexusAvaloniaModule))]
public class NexusAvaloniaBrowserModule : AbpModule { }
