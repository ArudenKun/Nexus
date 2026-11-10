using Configuration.Writable;
using Configuration.Writable.FormatProvider;
using Fluently;
using Microsoft.Extensions.DependencyInjection;
using Nexus.EntityFrameworkCore;
using Nexus.Settings;
using Nexus.ViewModels;
using Nexus.Views;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Nexus;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(FluentlyAvaloniaModule),
    typeof(NexusApplicationModule),
    typeof(NexusEntityFrameworkCoreModule)
)]
public class NexusAvaloniaModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddWritableOptions(builder =>
        {
            var appPaths = context.Services.GetSingletonInstance<AppPaths>();
            builder.UseCustomDirectory(appPaths.Config).AddFilePath("settings.json");
            builder.Add<ApplicationSettings>(c =>
            {
                c.SectionName = ApplicationSettings.SectionName;
                c.FormatProvider = new JsonAotFormatProvider(
                    ApplicationSettings.ApplicationSettingsJsonSerializerContext.Default
                );
            });
        });
        context.Services.AddConventionalRegistrar(new ViewModelConventionalRegistrar());
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ViewRegistryConfigurator.Register();
        ConfigureAvaloniaServices(context);
    }

    private void ConfigureAvaloniaServices(ServiceConfigurationContext context)
    {
        var containerBuilder = context.Services.GetContainerBuilder();
    }
}
