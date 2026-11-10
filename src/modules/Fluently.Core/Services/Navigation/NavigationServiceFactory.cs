using Fluently.Abstractions.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Fluently.Services.Navigation;

internal sealed class NavigationServiceFactory(IServiceProvider serviceProvider)
    : INavigationServiceFactory
{
    /// <inheritdoc />
    public INavigationService Create() =>
        ActivatorUtilities.CreateInstance<NavigationService>(serviceProvider);
}
