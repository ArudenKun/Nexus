using Nexus.Fluent.Abstractions.Navigation;

namespace Nexus.ViewModels;

public class MainViewModel : NavigableViewModel, ICanNavigateFrom
{
    public Task<bool> CanNavigateFromAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
