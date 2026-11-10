using Avalonia;
using Fluently.Abstractions.Theme;
using Fluently.Services;
using Fluently.Views;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Fluently;

[DependsOn(typeof(FluentlyCoreModule))]
public class FluentlyAvaloniaModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddConventionalRegistrar(new ViewConventionalRegistrar());
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IThemeService>(static _ => new ThemeService(
            Application.Current
                ?? throw new InvalidOperationException(
                    "The IThemeService was resolved before Avalonia created the application."
                )
        ));
    }
}
