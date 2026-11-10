using System.Threading.Tasks;
using Humanizer;
using Nexus.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;

namespace Nexus.ViewModels;

public class MainViewModel : ViewModel, ISingletonDependency
{
    public MainViewModel(ILocalEventBus localEventBus)
    {
        localEventBus.Subscribe<SetGlobalBusyEvent>(async message =>
            await SetBusyAsync(
                async () => await Task.Delay(message.Duration ?? 2.Seconds()),
                message.Message
            )
        );
    }
}
