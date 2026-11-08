using Volo.Abp.Autofac;
using Volo.Abp.Avalonia;
using Volo.Abp.Modularity;

namespace Nexus;

[DependsOn(typeof(AbpAvaloniaModule), typeof(NexusAvaloniaModule))]
public class NexusAvaloniaDesktopModule : AbpModule { }
