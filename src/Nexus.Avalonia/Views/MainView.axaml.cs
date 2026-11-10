using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Fluently.Abstractions.Navigation;
using Lucide.Avalonia;
using Nexus.Helpers;
using Nexus.ViewModels;
using Nexus.ViewModels.Pages;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

namespace Nexus.Views;

public partial class MainView : UserControl, ISingletonDependency
{
    public MainView(
        MainViewModel viewModel,
        NavigationPageFactory navigationPageFactory,
        IEnumerable<PageViewModel> pageViewModels
    )
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        PopulateNavigation(pageViewModels);

        FrameView.NavigationPageFactory = navigationPageFactory;
        viewModel.NavigationService.Navigated += NavigationServiceOnNavigated;
        NavView.SelectionChanged += NavViewOnSelectionChanged;
    }

    private void NavViewOnSelectionChanged(
        object? sender,
        FANavigationViewSelectionChangedEventArgs e
    )
    {
        if (e.SelectedItem is not FANavigationViewItem nvi)
            return;

        var target = nvi.Name == "SettingsItem" ? typeof(SettingsPageViewModel) : nvi.Tag as Type;

        if (target is null)
            return;

        DispatchHelpers.WaitOnDispatcher(ViewModel.NavigationService.NavigateToAsync(target));
    }

    private void NavigationServiceOnNavigated(object? sender, NavigatedEventArgs e)
    {
        FrameView.NavigateFromObject(
            e.Page,
            new FAFrameNavigationOptions { IsNavigationStackEnabled = true }
        );
    }

    protected MainViewModel ViewModel { get; }

    private void PopulateNavigation(IEnumerable<PageViewModel> pageViewModels)
    {
        var itemsByFooter = pageViewModels.OrderBy(x => x.Order).ToLookup(page => page.IsFooter);

        NavView.MenuItemsSource = itemsByFooter[false].Select(CreateNavigationViewItem);
        NavView.FooterMenuItemsSource = itemsByFooter[true].Select(CreateNavigationViewItem);
    }

    private static FANavigationViewItem CreateNavigationViewItem(PageViewModel pageViewModel) =>
        new()
        {
            Content = pageViewModel.Name,
            IconSource = new FAPathIconSource
            {
                Data = Geometry.Parse(pageViewModel.IconKind.GetGeometryData()),
            },
            IsVisible = pageViewModel.IsVisible,
            Tag = ProxyHelper.GetUnProxiedType(pageViewModel),
        };

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        NavView.SelectionChanged -= NavViewOnSelectionChanged;
        ViewModel.NavigationService.Navigated -= NavigationServiceOnNavigated;
    }
}
