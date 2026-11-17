using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Humanizer;
using Microsoft.Extensions.Logging;
using Nexus.Books;
using Nexus.Events;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Services;
using Volo.Abp.Application.Dtos;

namespace Nexus.ViewModels.Pages;

public partial class HomePageViewModel : PageViewModel, INavigatedTo
{
    public HomePageViewModel(IBookAppService bookAppService, AuthService authService)
    {
        BookAppService = bookAppService;
        AuthService = authService;
        Order = 1;
    }

    public override FASymbol IconSymbol => FASymbol.HomeFilled;

    protected IBookAppService BookAppService { get; }
    protected AuthService AuthService { get; }

    [ObservableProperty]
    public partial string Data { get; set; } = string.Empty;

    [RelayCommand]
    private async Task SetBusyShellAsync()
    {
        Logger.LogInformation("SetGlobalBusyAsync Pressed");
        await SetBusyAsync(async () => await Task.Delay(3.Seconds()), "Loading...");
        // var duration = Random.Shared.Next(2, 5).Seconds();
        // await LocalEventBus.PublishAsync(new BusyShellEvent(true, duration));
    }

    [RelayCommand]
    private async Task ManualExpireTokenAsync()
    {
        AuthService.ManualExpire();
    }

    public async Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        var data = await BookAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        Data = JsonSerializer.Serialize(data);
    }
}
