using Microsoft.Extensions.DependencyInjection;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Fluent.Services.Navigation;

namespace Nexus.Fluent.Extensions;

/// <summary>
///     Registers Fluently in a service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="services">The service collection to register Fluently in.</param>
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNavigation(string key) =>
            services.AddKeyedSingleton<INavigationService, NavigationService>(key);
    }
}
