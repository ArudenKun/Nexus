using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Avalonia.Extensions;

public static class HostExtensions
{
    extension(IHost host)
    {
        public void InitializeApplication()
        {
            var abpApplication = InitializeCore(host);
            abpApplication.Initialize(host.Services);
        }

        public async Task InitializeApplicationAsync()
        {
            var abpApplication = InitializeCore(host);
            await abpApplication.InitializeAsync(host.Services);
        }
    }

    private static IAbpApplicationWithExternalServiceProvider InitializeCore(IHost host)
    {
        Check.NotNull(host, nameof(host));
        host.Services.GetRequiredService<ObjectAccessor<IHost>>().Value = host;
        var abpApplication =
            host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>();
        var hostLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        hostLifetime.ApplicationStopping.Register(abpApplication.Shutdown);
        hostLifetime.ApplicationStopped.Register(abpApplication.Dispose);
        return abpApplication;
    }
}
