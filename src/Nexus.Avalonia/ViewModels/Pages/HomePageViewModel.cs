using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lucide.Avalonia;
using Nexus.Messages;

namespace Nexus.ViewModels.Pages;

public partial class HomePageViewModel : PageViewModel
{
    public HomePageViewModel()
    {
        Order = 1;
    }

    public override LucideIconKind IconKind => LucideIconKind.House;

    [RelayCommand]
    private void SetBusy()
    {
        LocalEventBus.PublishAsync(new SetGlobalBusyMessage("Testing..."), false);
    }
}
