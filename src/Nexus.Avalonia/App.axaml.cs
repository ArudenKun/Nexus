using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Configuration.Writable;
using Fluently;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Settings;
using Nexus.ViewModels;
using Nexus.ViewModels.Pages;
using Nexus.Views;
using Volo.Abp;

namespace Nexus;

public class App : Application
{
    public App(IAbpApplicationWithExternalServiceProvider abpApplication)
    {
        AbpApplication = abpApplication;
    }

    public App(IAbpApplicationWithInternalServiceProvider abpApplication)
    {
        AbpApplication = abpApplication;
    }

    protected IAbpApplication AbpApplication { get; }
    protected IServiceProvider ServiceProvider => AbpApplication.ServiceProvider;
    protected IThemeService ThemeService => ServiceProvider.GetRequiredService<IThemeService>();

    protected IWritableOptions<ApplicationSettings> ApplicationOptions =>
        ServiceProvider.GetRequiredService<IWritableOptions<ApplicationSettings>>();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (
            AbpApplication
            is IAbpApplicationWithInternalServiceProvider abpApplicationWithInternalServiceProvider
        )
            await abpApplicationWithInternalServiceProvider.InitializeAsync();

        ThemeService.Theme = ApplicationOptions.CurrentValue.Theme;

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:

                desktop.MainWindow = new MainWindow(ServiceProvider)
                {
                    DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>(),
                    Content = ServiceProvider.GetRequiredService<MainView>(),
                };
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = ServiceProvider.GetRequiredService<MainView>();
                singleViewPlatform.MainView.Unloaded += (_, _) =>
                {
                    AbpApplication.Shutdown();
                    AbpApplication.Dispose();
                };
                break;
        }

        await ServiceProvider
            .GetRequiredService<INavigationService>()
            .NavigateToAsync<HomePageViewModel>();

        base.OnFrameworkInitializationCompleted();
    }
}
