using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Fluent.Abstractions.Dialogs;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Fluent.Abstractions.Theme;
using Nexus.Fluent.Abstractions.Toasts;
using Nexus.Fluent.Abstractions.Views;
using Nexus.Fluent.Services;
using Nexus.Fluent.Services.Dialogs;
using Nexus.Fluent.Services.Navigation;
using Nexus.Fluent.Services.Toasts;
using Nexus.Fluent.Views;
using Volo.Abp.Modularity;
using Volo.Abp.Reflection;

namespace Nexus.Fluent;

public class NexusAvaloniaFluentModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddConventionalRegistrar(new ViewConventionalRegistrar());
    }

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
        context.Services.AddSingleton(TimeProvider.System);
        context.Services.AddSingleton<IToastService, ToastService>();

        context.Services.AddSingleton<IThemeService>(static _ => new ThemeService(
            Application.Current
                ?? throw new InvalidOperationException(
                    "The IThemeService was resolved before Avalonia created the application."
                )
        ));
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        AddViews(context.Services);
    }

    private static void AddViews(IServiceCollection services)
    {
        services.OnRegistered(ctx =>
        {
            if (
                !ReflectionHelper.IsAssignableToGenericType(
                    ctx.ImplementationType,
                    typeof(IViewFor<>)
                )
            )
                return;

            var typeInfo = ctx.ImplementationType.GetTypeInfo();
            var viewForInterface = typeInfo.ImplementedInterfaces.FirstOrDefault(x =>
                x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IViewFor<>)
            );

            if (viewForInterface is null)
                return;

            var viewModelType = viewForInterface.GetGenericArguments()[0];

            if (ViewRegistry.Contains(viewModelType))
                return;

            ViewRegistry.Add(viewModelType, (sp, vm) =>
            {
                var view = (Control)sp.GetRequiredService(ctx.ImplementationType);
                view.DataContext = vm;
                return (IViewFor<object>)view;
            });
        });
    }
}
