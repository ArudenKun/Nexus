using Configuration.Writable;
using Configuration.Writable.FormatProvider;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Core;
using Nexus.Fluent;
using Nexus.Fluent.Extensions;
using Nexus.Options;
using Nexus.Services;
using Nexus.ViewModels;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Nexus;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(NexusHttpApiClientModule),
    typeof(NexusAvaloniaHostingModule),
    typeof(NexusAvaloniaFluentModule)
)]
public class NexusAvaloniaModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddConventionalRegistrar(new ViewModelConventionalRegistrar());
        context.Services.AddWritableOptions(builder =>
        {
            var appPaths = context.Services.GetSingletonInstance<AppPaths>();
            builder.Add<AuthOptions>(c =>
            {
                c.UseExecutableDirectory().AddFilePath("appsettings.json");
                c.SectionName = AuthOptions.SectionName;
                c.FormatProvider = new JsonAotFormatProvider(
                    AuthOptions.AuthOptionsSerializerContext.Default
                );
                c.UseDataAnnotationsValidation = true;
            });
            builder.Add<ApplicationOptions>(c =>
            {
                c.UseCustomDirectory(appPaths.Config).AddFilePath("settings.json");
                c.SectionName = ApplicationOptions.SectionName;
                c.FormatProvider = new JsonAotFormatProvider(
                    ApplicationOptions.ApplicationSerializerContext.Default
                );
                c.UseDataAnnotationsValidation = true;
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddNavigation("Page");

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<NexusAvaloniaModule>("Nexus");
        });

        context.Services.AddHostedService<AuthStateWatcherHostedService>();
    }
}
