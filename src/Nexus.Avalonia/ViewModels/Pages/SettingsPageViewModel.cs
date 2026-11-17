using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.Events;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Fluent.Abstractions.Theme;
using Volo.Abp.DependencyInjection;

namespace Nexus.ViewModels.Pages;

public partial class SettingsPageViewModel : PageViewModel, INavigatedTo, ISingletonDependency
{
    public SettingsPageViewModel()
    {
        IsFooter = true;
        IsVisible = false;
    }

    protected IThemeService ThemeService => LazyServiceProvider.GetRequiredService<IThemeService>();

    [ObservableProperty]
    public partial ApplicationTheme Theme { get; set; }

    public ApplicationTheme[] Themes => field ??= Enum.GetValues<ApplicationTheme>();

    public override FASymbol IconSymbol => FASymbol.Settings;

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task ChangeThemeAsync(CancellationToken cancellationToken)
    {
        if (Theme == ThemeService.ActualTheme)
            return;

        var previous = ThemeService.Theme;

        // Optimistically apply so the UI responds immediately (theme switches are cheap),
        // then persist; roll back if persistence fails.
        ThemeService.Theme = Theme;

        try
        {
            await ApplicationOptions.SaveAsync(s => s.Theme = Theme, cancellationToken);
            await LocalEventBus.PublishAsync(new ThemeChangedEvent(Theme));
        }
        catch (Exception ex)
        {
            ThemeService.Theme = previous;
            // Surface the failure — adjust to your logging/dialog strategy.
            Logger.LogError(ex, "Failed to persist theme change to {Theme}.", Theme);
        }
    }

    public Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        Theme = ThemeService.ActualTheme;
        return Task.CompletedTask;
    }
}
