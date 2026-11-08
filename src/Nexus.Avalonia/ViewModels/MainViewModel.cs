using System.Threading.Tasks;
using Humanizer;
using Nexus.Messages;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;

namespace Nexus.ViewModels;

public partial class MainViewModel : ViewModel, ISingletonDependency
{
    public MainViewModel(ILocalEventBus localEventBus)
    {
        localEventBus.Subscribe<SetGlobalBusyMessage>(async message =>
            await SetBusyAsync(
                async () => await Task.Delay(message.Duration ?? 2.Seconds()),
                message.Message
            )
        );
    }
}
