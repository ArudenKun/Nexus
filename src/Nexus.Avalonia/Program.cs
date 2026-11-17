using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexus.Core;
using Nexus.Extensions;
using Serilog;
using Serilog.Core;
using Volo.Abp.Autofac;

namespace Nexus;

public static class Program
{
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
            : AppPaths.For(NexusConsts.AppName);
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
                (sp, appBuilder) =>
                    appBuilder
                        .UsePlatformDetect()
                        .UseHotReload()
                        .UseServiceProvider(sp)
                        .UseComponentControlFactory(t =>
                            (Control)ActivatorUtilities.GetServiceOrCreateInstance(sp, t)
                        )
                        .LogToTrace(),
                appBuilder => appBuilder.StartWithClassicDesktopLifetime(args)
            );

            builder.AddApplication<NexusAvaloniaModule>();

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
}
