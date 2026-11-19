using Avalonia.Collections;
using CommunityToolkit.Mvvm.Input;
using DisposableGenerator;
using FluentAvalonia.UI.Controls;
using Lucide.Avalonia;
using Nexus.Events;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Fluent.Extensions;
using Nexus.Services;
using Nexus.ViewModels.Dialogs;
using Nexus.ViewModels.Pages;
using Volo.Abp.DynamicProxy;

namespace Nexus.ViewModels;

[GenerateDisposable]
public partial class MainViewModel : NavigableViewModel, INavigatedTo, ICanNavigateFrom
{
    public MainViewModel(IEnumerable<PageViewModel> pageViewModels, AuthService authService)
    {
        AuthService = authService;
        Pages = [.. pageViewModels];
        PopulatePages();
    }

    protected IReadOnlyList<PageViewModel> Pages { get; }

    public IAvaloniaList<FANavigationViewItem> MenuPages { get; } =
        new AvaloniaList<FANavigationViewItem>();

    public IAvaloniaList<FANavigationViewItem> FooterMenuPages { get; } =
        new AvaloniaList<FANavigationViewItem>();

    protected AuthService AuthService { get; }

    private void PopulatePages()
    {
        var itemsByFooter = Pages.OrderBy(x => x.Order).ToLookup(page => page.IsFooter);
        MenuPages.AddRange(itemsByFooter[false].Select(CreateNavigationViewItem));
        FooterMenuPages.AddRange([
            .. itemsByFooter[true].Select(CreateNavigationViewItem),
            new FANavigationViewItem
            {
                Name = "Logout",
                Content = "Logout",
                IconSource = new FAPathIconSource().Data(
                    StreamGeometry.Parse(LucideIconKind.LogOut.GetGeometryData())
                ),
            },
        ]);
    }

    private static FANavigationViewItem CreateNavigationViewItem(PageViewModel pageViewModel) =>
        new()
        {
            Content = pageViewModel.Name,
            IconSource = new FASymbolIconSource { Symbol = pageViewModel.IconSymbol },
            IsVisible = pageViewModel.IsVisible,
            Tag = ProxyHelper.GetUnProxiedType(pageViewModel),
        };

    [RelayCommand]
    private async Task ChangePageEventAsync(
        FANavigationViewSelectionChangedEventArgs args,
        CancellationToken cancellationToken
    )
    {
        if (args.SelectedItem is not FANavigationViewItem nvi)
            return;

        if (nvi.Name == "Logout")
        {
            await LogoutAsync(cancellationToken);
            return;
        }

        var target = nvi.Name == "SettingsItem" ? typeof(SettingsPageViewModel) : nvi.Tag as Type;
        if (target is null)
            return;

        await PageNavigationService.NavigateToAsync(target, cancellationToken);
    }

    private async Task LogoutAsync(CancellationToken cancellationToken)
    {
        var request = new ConfirmRequest("Logout", "Are you sure you want to logout?", "Confirm");
        var result = await DialogService.ShowAsync<ConfirmDialogViewModel, bool, ConfirmRequest>(
            request,
            cancellationToken
        );

        if (!result.Value)
        {
            await PageNavigationService.GoBackAsync(cancellationToken);
            return;
        }

        await LocalEventBus.PublishAsync(new BusyShellEvent(true));
        await AuthService.LogoutAsync(cancellationToken);
    }

    public Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<bool> CanNavigateFromAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
