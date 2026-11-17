using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Events;
using Nexus.Fluent.Extensions;
using Nexus.Services;

namespace Nexus.ViewModels;

public partial class LoginViewModel : NavigableViewModel
{
    public LoginViewModel()
    {
        UserId = "";
        Password = "";
#if DEBUG
        UserId = "admin";
        Password = "1q2w3E*";
#endif
    }

    protected AuthService AuthService => LazyServiceProvider.GetRequiredService<AuthService>();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginWithCredentialsCommand))]
    public partial string UserId { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginWithCredentialsCommand))]
    public partial string Password { get; set; }

    [RelayCommand]
    private async Task LoginWithBrowserAsync()
    {
        try
        {
            await LocalEventBus.PublishAsync(new BusyShellEvent(true, "Loading"));
            var authResult = await AuthService.LoginAsync();
            if (!authResult.Succeeded)
            {
                ToastService.ShowError(
                    "Login Failed",
                    "An error occurred while logging in the browser"
                );
                return;
            }

            await NavigationService.NavigateToAsync<MainViewModel>();
        }
        finally
        {
            await LocalEventBus.PublishAsync(new BusyShellEvent(false));
        }
    }

    private bool CanLoginWithCredentials => !UserId.IsNullOrEmpty() && !Password.IsNullOrEmpty();

    [RelayCommand(CanExecute = nameof(CanLoginWithCredentials))]
    private async Task LoginWithCredentialsAsync()
    {
        try
        {
            await LocalEventBus.PublishAsync(new BusyShellEvent(true, "Loading"));
            var authResult = await AuthService.LoginAsync(UserId, Password);
            if (!authResult.Succeeded)
            {
                ToastService.ShowError("Login Failed", "Invalid User Id or Password");
                return;
            }

            await NavigationService.NavigateToAsync<MainViewModel>();
        }
        finally
        {
            await LocalEventBus.PublishAsync(new BusyShellEvent(false));
        }
    }
}
