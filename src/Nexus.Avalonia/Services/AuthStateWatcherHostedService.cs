using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexus.Events;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Fluent.Abstractions.Toasts;
using Nexus.Fluent.Extensions;
using Nexus.ViewModels;
using Volo.Abp.EventBus.Local;

namespace Nexus.Services;

public class AuthStateWatcherHostedService : IHostedService
{
    public AuthStateWatcherHostedService(
        AuthService authService,
        ILocalEventBus localEventBus,
        IServiceProvider serviceProvider
    )
    {
        AuthService = authService;
        LocalEventBus = localEventBus;
        ServiceProvider = serviceProvider;
    }

    protected AuthService AuthService { get; }
    protected ILocalEventBus LocalEventBus { get; }
    protected IServiceProvider ServiceProvider { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        AuthService.TokenExpired += AuthServiceOnTokenExpired;
        AuthService.Logout += AuthServiceOnLogout;
    }

    private void AuthServiceOnTokenExpired(object? sender, EventArgs args)
    {
        var navigationService = ServiceProvider.GetRequiredService<INavigationService>();
        if (navigationService.CurrentPage is LoginViewModel)
            return;

        NavigateToLogin();
    }

    private void AuthServiceOnLogout(object? sender, EventArgs e) => NavigateToLogin();

    private void NavigateToLogin()
    {
        var navigationService = ServiceProvider.GetRequiredService<INavigationService>();
        var toastService = ServiceProvider.GetRequiredService<IToastService>();

        navigationService
            .NavigateToAsync<LoginViewModel>()
            .ContinueWith(task =>
            {
                LocalEventBus.PublishAsync(new BusyShellEvent(false));
                if (task.Result)
                    toastService.ShowWarning("Logout", "You have been logged out.");
            });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        AuthService.TokenExpired -= AuthServiceOnTokenExpired;
        AuthService.Logout -= AuthServiceOnLogout;
        return Task.CompletedTask;
    }
}
