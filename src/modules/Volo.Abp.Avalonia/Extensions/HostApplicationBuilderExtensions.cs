using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Avalonia.Internals;
using Volo.Abp.Modularity;

namespace Volo.Abp.Avalonia.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddAvalonia<TApplication>(
        this IHostApplicationBuilder builder,
        Action<AppBuilder> configureAppBuilder,
        Func<AppBuilder, int> startLifetime
    )
        where TApplication : Application =>
        builder.AddAvalonia<TApplication>(
            (_, appBuilder) => configureAppBuilder(appBuilder),
            startLifetime
        );

    public static IHostApplicationBuilder AddAvalonia<TApplication>(
        this IHostApplicationBuilder builder,
        Action<AppBuilder> configureAppBuilder
    )
        where TApplication : Application =>
        builder.AddAvalonia<TApplication>(
            (_, appBuilder) => configureAppBuilder(appBuilder),
            appBuilder => appBuilder.StartWithClassicDesktopLifetime([])
        );

    public static IHostApplicationBuilder AddAvalonia<TApplication>(
        this IHostApplicationBuilder builder,
        Action<IServiceProvider, AppBuilder> configureAppBuilder
    )
        where TApplication : Application =>
        builder.AddAvalonia<TApplication>(
            configureAppBuilder,
            appBuilder => appBuilder.StartWithClassicDesktopLifetime([])
        );

    public static IHostApplicationBuilder AddAvalonia<TApplication>(
        this IHostApplicationBuilder builder,
        Action<IServiceProvider, AppBuilder> configureAppBuilder,
        Func<AppBuilder, int> startLifetime
    )
        where TApplication : Application
    {
        builder.Services.AddSingleton<TApplication>();
        builder.Services.AddSingleton(sp =>
        {
            var appBuilder = AppBuilder.Configure(sp.GetRequiredService<TApplication>);
            configureAppBuilder(sp, appBuilder);
            return appBuilder;
        });
        builder.Services.AddSingleton(startLifetime);
        builder.Services.AddSingleton<AvaloniaThread>();
        builder.Services.AddHostedService<AvaloniaHostedService>();
        return builder;
    }

    public static IHostApplicationBuilder AddApplication<TStartupModule>(
        this IHostApplicationBuilder builder,
        Action<AbpApplicationCreationOptions>? optionsAction = null
    )
        where TStartupModule : IAbpModule
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddApplication<TStartupModule>(options =>
            ConfigureOptions(builder, options, optionsAction)
        );
        return builder;
    }

    public static async Task<IHostApplicationBuilder> AddApplicationAsync<TStartupModule>(
        this IHostApplicationBuilder builder,
        Action<AbpApplicationCreationOptions>? optionsAction = null
    )
        where TStartupModule : IAbpModule
    {
        ArgumentNullException.ThrowIfNull(builder);
        await builder.Services.AddApplicationAsync<TStartupModule>(options =>
            ConfigureOptions(builder, options, optionsAction)
        );
        return builder;
    }

    private static void ConfigureOptions(
        IHostApplicationBuilder builder,
        AbpApplicationCreationOptions options,
        Action<AbpApplicationCreationOptions>? userOptionsAction
    )
    {
        options.Services.ReplaceConfiguration(builder.Configuration);
        userOptionsAction?.Invoke(options);

        if (string.IsNullOrWhiteSpace(options.Environment))
        {
            options.Environment = builder.Environment.EnvironmentName;
        }
    }
}
