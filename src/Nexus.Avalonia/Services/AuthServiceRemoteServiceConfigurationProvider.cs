using Microsoft.Extensions.Options;
using Nexus.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;

namespace Nexus.Services;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IRemoteServiceConfigurationProvider))]
public class AuthServiceRemoteServiceConfigurationProvider
    : IRemoteServiceConfigurationProvider,
        IScopedDependency
{
    protected AbpRemoteServiceOptions Options { get; }
    protected AuthOptions AuthOptions { get; }

    public AuthServiceRemoteServiceConfigurationProvider(
        IOptionsMonitor<AbpRemoteServiceOptions> options,
        IOptions<AuthOptions> authOptions
    )
    {
        Options = options.CurrentValue;
        AuthOptions = authOptions.Value;
    }

    public Task<RemoteServiceConfiguration> GetConfigurationOrDefaultAsync(string name)
    {
        return Task.FromResult(new RemoteServiceConfiguration(AuthOptions.Authority));
    }

    public Task<RemoteServiceConfiguration?> GetConfigurationOrDefaultOrNullAsync(string name)
    {
        return Task.FromResult<RemoteServiceConfiguration?>(
            new RemoteServiceConfiguration(AuthOptions.Authority)
        );
    }
}
