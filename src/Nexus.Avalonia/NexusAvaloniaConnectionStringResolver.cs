using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Nexus;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IConnectionStringResolver))]
public class NexusAvaloniaConnectionStringResolver : IConnectionStringResolver, ITransientDependency
{
    private readonly AppPaths _appPaths;

    public NexusAvaloniaConnectionStringResolver(AppPaths appPaths)
    {
        _appPaths = appPaths;
    }

    public string Resolve(string? connectionStringName = null)
    {
        return _appPaths.DatabasePath;
    }

    public Task<string> ResolveAsync(string? connectionStringName = null)
    {
        return Task.FromResult(_appPaths.DatabasePath);
    }
}
