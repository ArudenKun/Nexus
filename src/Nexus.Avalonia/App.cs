using Avalonia.Svg.Skia;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;
using FluentAvalonia.Styling;
using Lucide.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.Events;
using Nexus.Extensions;
using Nexus.Fluent;
using Nexus.Fluent.Views;
using Nexus.Services;
using Nexus.Views;
using Volo.Abp.EventBus.Local;

[assembly: GenerateMarkupExtensionsForAvalonia(true)]
[assembly: GenerateFactoriesForAssembly(typeof(FluentAvaloniaTheme))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(FluentAvaloniaTheme))]
[assembly: GenerateFactoriesForAssembly(typeof(NexusAvaloniaFluentModule))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(NexusAvaloniaFluentModule))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(SvgImage))]
[assembly: GenerateFactoriesForAssembly(typeof(LucideIcon))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(LucideIcon))]
[assembly: GenerateMarkupExtensionsForAssembly(typeof(IBehavior))]

[assembly: GenerateStaticResources("AvaloniaResources")]

namespace Nexus;

public class App : Application
{
    private bool _isShuttingDown;

    public override void Initialize()
    {
        Styles.Add(new FluentAvaloniaTheme());
        DataTemplates.Add(new ViewLocator());
    }

    public static new App Current =>
        (App?)Application.Current ?? throw new InvalidOperationException("App.Current not found");
    public static new IClassicDesktopStyleApplicationLifetime ApplicationLifetime =>
        (IClassicDesktopStyleApplicationLifetime?)Application.Current?.ApplicationLifetime
        ?? throw new InvalidOperationException("App.Current not found");

    public IServiceProvider ServiceProvider { get; init; } = null!;

    public override void OnFrameworkInitializationCompleted()
    {
        ApplicationLifetime.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        ApplicationLifetime.ShutdownRequested += DesktopLifetimeOnShutdownRequested;

        ServiceProvider
            .GetRequiredService<ILocalEventBus>()
            .PublishAsync(new AvaloniaReadyEvent())
            .WaitOnDispatcher();

        base.OnFrameworkInitializationCompleted();
    }

    private async void DesktopLifetimeOnShutdownRequested(
        object? sender,
        ShutdownRequestedEventArgs e
    )
    {
        if (_isShuttingDown)
            return;

        _isShuttingDown = true;
        e.Cancel = true;

        try
        {
            var authService = ServiceProvider.GetRequiredService<AuthService>();
            await authService.LogoutAsync().WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            ServiceProvider
                .GetRequiredService<ILogger<App>>()
                .LogWarning(ex, "An error occured during auth cleanup");
        }
        finally
        {
            if (sender is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
        }
    }
}
