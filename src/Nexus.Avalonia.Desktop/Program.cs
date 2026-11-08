using System;
using System.IO;
using Autofac;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Avalonia.Extensions;

namespace Nexus;

public static class Program
{
    private static readonly IAbpApplicationWithInternalServiceProvider DebugAbpApplication =
        AbpApplicationFactory.Create<NexusAvaloniaModule>(options =>
        {
            options.UseAutofac();
        });

    private const bool IsDebug
#if DEBUG
    = true;
#else
    = false;
#endif

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var isPortable = File.Exists(".portable") || IsDebug;
        var appPaths = isPortable
            ? AppPaths.Portable(AppContext.BaseDirectory)
            : AppPaths.For(NexusConsts.Name);
        Environment.SetEnvironmentVariable(
            "LOG_BASE_PATH",
            appPaths.Logs,
            EnvironmentVariableTarget.Process
        );

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(c => c.File(Path.Combine(appPaths.Logs, "logs.txt")))
            .WriteTo.Async(c => c.Console())
            .CreateBootstrapLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton(appPaths);

            builder.Services.AddSerilog(
                (sp, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(builder.Configuration)
                        .ReadFrom.Services(sp)
            );

            var containerBuilder = new ContainerBuilder();

            builder.ConfigureContainer(new AbpAutofacServiceProviderFactory(containerBuilder));
            builder.Services.AddObjectAccessor(containerBuilder);

            builder.AddAvalonia<App>(
                appBuilder => appBuilder.UsePlatformDetect().WithInterFont().LogToTrace(),
                appBuilder => appBuilder.StartWithClassicDesktopLifetime(args)
            );

            builder.AddApplication<NexusAvaloniaDesktopModule>();

            var host = builder.Build();
            host.InitializeApplication();
            host.Run();
        }
        catch (Exception ex)
        {
            Log.ForContext(Constants.SourceContextPropertyName, "Nexus")
                .Fatal(ex, "Host terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    // ReSharper disable once UnusedMember.Global
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure(() =>
            {
                DebugAbpApplication.Services.AddSingleton<App>();
                DebugAbpApplication.Initialize();
                return DebugAbpApplication.ServiceProvider.GetRequiredService<App>();
            })
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
