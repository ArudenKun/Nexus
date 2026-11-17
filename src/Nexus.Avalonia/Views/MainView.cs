using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Lucide.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Events;
using Nexus.Extensions;
using Nexus.Extensions.Controls;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Fluent.Abstractions.Views;
using Nexus.Fluent.Extensions;
using Nexus.Services;
using Nexus.ViewModels;
using Nexus.ViewModels.Pages;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.EventBus.Local;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views;

public class MainView : View<MainViewModel>, ISingletonDependency
{
    private FANavigationView _navView = null!;
    private FAFrame _frameView = null!;

    public MainView(MainViewModel viewModel)
    {
        DataContext = viewModel;
    }

    protected override void OnInitialized()
    {
        _frameView.NavigationPageFactory = new NavigationPageFactory(
            ServiceProvider.GetRequiredService<IViewProvider>()
        );
        ViewModel.PageNavigationService.Navigated += PageNavigationServiceOnNavigated;
        _navView.SelectionChanged += NavViewOnSelectionChanged;
        _navView.BackRequested += NavViewOnBackRequested;

        ViewModel.PageNavigationService.NavigateToAsync<HomePageViewModel>().WaitOnDispatcher();
    }

    private void NavViewOnBackRequested(object? sender, FANavigationViewBackRequestedEventArgs e)
    {
        if (_frameView.CanGoBack)
        {
            _frameView.GoBack();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _navView.SelectionChanged -= NavViewOnSelectionChanged;
        _navView.BackRequested -= NavViewOnBackRequested;
        ViewModel.PageNavigationService.Navigated -= PageNavigationServiceOnNavigated;
    }

    private void NavViewOnSelectionChanged(
        object? sender,
        FANavigationViewSelectionChangedEventArgs e
    )
    {
        if (e.SelectedItem is not FANavigationViewItem nvi)
            return;

        if (nvi.Name == "Logout")
        {
            LazyServiceProvider
                .GetRequiredService<ILocalEventBus>()
                .PublishAsync(new BusyShellEvent(true))
                .WaitOnDispatcher();
            LazyServiceProvider.GetRequiredService<AuthService>().LogoutAsync().WaitOnDispatcher();
            return;
        }

        var target = nvi.Name == "SettingsItem" ? typeof(SettingsPageViewModel) : nvi.Tag as Type;

        if (target is null)
            return;

        ViewModel.PageNavigationService.NavigateToAsync(target).WaitOnDispatcher();
    }

    private void PageNavigationServiceOnNavigated(object? sender, NavigatedEventArgs e)
    {
        _frameView.NavigateFromObject(
            e.Page,
            new FAFrameNavigationOptions { IsNavigationStackEnabled = true }
        );
    }

    private void PopulateNavigation(IEnumerable<PageViewModel> pageViewModels)
    {
        var itemsByFooter = pageViewModels.OrderBy(x => x.Order).ToLookup(page => page.IsFooter);

        _navView.MenuItemsSource = itemsByFooter[false].Select(CreateNavigationViewItem);
        _navView.FooterMenuItemsSource = itemsByFooter[true]
            .Select(CreateNavigationViewItem)
            .Concat([
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

    protected override object Build(MainViewModel vm) =>
        BusyArea()
            .IsBusy(vm, x => x.IsBusy, BindingMode.OneWay)
            .BusyText(vm, x => x.BusyText, BindingMode.OneWay)
            .Content(
                FANavigationView()
                    .AlwaysShowHeader(true)
                    .PaneDisplayMode(FANavigationViewPaneDisplayMode.Left)
                    .Name("NavView", Scope)
                    .Ref(out _navView)
                    .IsSettingsVisible(true)
                    .IsBackButtonVisible(true)
                    .IsBackEnabled(false)
                    .Content(
                        new Border()
                            .BorderBrush("CardStrokeColorDefaultBrush".GetDynamicResource())
                            .BorderThickness(1)
                            .CornerRadius(8)
                            .Child(
                                new FAFrame()
                                    .Name("FrameView", Scope)
                                    .Ref(out _frameView)
                                    .OnNavigated(
                                        (_, _) => _navView.IsBackEnabled = _frameView.CanGoBack
                                    )
                            )
                    )
            );

    private class NavigationPageFactory : IFANavigationPageFactory
    {
        private readonly IViewProvider _viewProvider;

        public NavigationPageFactory(IViewProvider viewProvider) => _viewProvider = viewProvider;

        public Control GetPage(Type srcType) => throw new NotImplementedException();

        public Control GetPageFromObject(object target) => (Control)_viewProvider.GetView(target);
    }
}
