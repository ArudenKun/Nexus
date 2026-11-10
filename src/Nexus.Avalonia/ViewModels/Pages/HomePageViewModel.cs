using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Lucide.Avalonia;
using Nexus.Events;

namespace Nexus.ViewModels.Pages;

public partial class HomePageViewModel : PageViewModel
{
    public HomePageViewModel()
    {
        Order = 1;
    }

    public override LucideIconKind IconKind => LucideIconKind.House;

    [RelayCommand]
    private async Task SetGlobalBusyAsync()
    {
        var duration = Random.Shared.Next(2, 5).Seconds();
        await LocalEventBus.PublishAsync(
            new SetGlobalBusyEvent($"Testing... ({duration.Humanize()})", duration),
            false
        );
    }
}
