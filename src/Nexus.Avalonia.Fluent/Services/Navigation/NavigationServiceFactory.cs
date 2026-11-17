using Microsoft.Extensions.DependencyInjection;
using Nexus.Fluent.Abstractions.Navigation;

namespace Nexus.Fluent.Services.Navigation;

internal sealed class NavigationServiceFactory(IServiceProvider serviceProvider)
    : INavigationServiceFactory
{
    /// <inheritdoc />
    public INavigationService Create() =>
        ActivatorUtilities.CreateInstance<NavigationService>(serviceProvider);
}
