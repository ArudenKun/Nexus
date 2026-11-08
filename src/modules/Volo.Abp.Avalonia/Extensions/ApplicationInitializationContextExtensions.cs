using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Avalonia.Extensions;

public static class ApplicationInitializationContextExtensions
{
    public static IHost GetHost(this ApplicationInitializationContext context)
    {
        var host = context.ServiceProvider.GetRequiredService<IObjectAccessor<IHost>>().Value;
        return host ?? throw new InvalidOperationException("Host is not initialized");
    }

    public static IServiceProvider GetHostServiceProvider(
        this ApplicationInitializationContext context
    )
    {
        var host = context.GetHost();
        return host.Services;
    }

    public static IHostEnvironment GetEnvironment(this ApplicationInitializationContext context)
    {
        return context.ServiceProvider.GetRequiredService<IHostEnvironment>();
    }
}
