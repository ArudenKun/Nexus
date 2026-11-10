using Fluently.Abstractions.Dialogs;
using Fluently.Abstractions.Navigation;
using Fluently.Abstractions.Toasts;
using Fluently.Abstractions.Views;
using Fluently.Services.Dialogs;
using Fluently.Services.Navigation;
using Fluently.Services.Toasts;
using Fluently.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace Fluently;

public class FluentlyCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton(static provider => new ViewModelScopes(provider));
        context.Services.AddSingleton(static provider => new ViewProvider(
            provider.GetRequiredService<ViewModelScopes>()
        ));
        context.Services.AddSingleton<IViewProvider>(static provider =>
            provider.GetRequiredService<ViewProvider>()
        );
        context.Services.AddSingleton<INavigationService, NavigationService>();
        context.Services.AddSingleton<INavigationServiceFactory, NavigationServiceFactory>();
        context.Services.AddSingleton<IDialogService, DialogService>();
        context.Services.TryAddSingleton(TimeProvider.System);
        context.Services.AddSingleton<IToastService, ToastService>();
    }
}
