using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using Humanizer;
using Microsoft.Extensions.Logging;
using Nexus.Data;
using Volo.Abp.DependencyInjection;

namespace Nexus.Views;

public class MainSplashScreen : IFAApplicationSplashScreen
{
    private readonly MainWindow _mainWindow;
    private readonly NexusDbMigrationService _dbMigrationService;

    public MainSplashScreen(MainWindow mainWindow, NexusDbMigrationService dbMigrationService)
    {
        _mainWindow = mainWindow;
        _dbMigrationService = dbMigrationService;
        Content = new MainSplashScreenContent();
        SplashScreenContent = Content;
    }

    public required IAbpLazyServiceProvider LazyServiceProvider { get; init; }

    protected IServiceProvider ServiceProvider =>
        LazyServiceProvider.LazyGetRequiredService<IServiceProvider>();

    protected ILoggerFactory LoggerFactory =>
        LazyServiceProvider.LazyGetRequiredService<ILoggerFactory>();

    protected ILogger Logger =>
        LazyServiceProvider.LazyGetService<ILogger>(_ =>
            LoggerFactory.CreateLogger(GetType().FullName!)
        );

    protected MainSplashScreenContent Content { get; }

    public string AppName => NexusConsts.Name;
    public IImage AppIcon =>
        new Bitmap(AssetLoader.Open(new Uri("avares://Nexus.Avalonia/Assets/icon.png")));
    public object SplashScreenContent { get; }
    public int MinimumShowTime => (int)4.Seconds().TotalMilliseconds;

    public async Task RunTasks(CancellationToken cancellationToken)
    {
        await MigrateDatabaseAsync(cancellationToken);
    }

    private async Task MigrateDatabaseAsync(CancellationToken cancellationToken)
    {
        Content.TextBlock.Text = "Migrating Database";
        try
        {
            await _dbMigrationService.MigrateAsync();
            Content.TextBlock.Text = "Migrating Database Finished";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Database migration failed");
            Content.TextBlock.Text = "Migration Failed";
            throw;
        }
    }
}
