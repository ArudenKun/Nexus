using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Windowing;
using Fluently;
using Lucide.Avalonia;
using Nexus.ViewModels;
using Nexus.ViewModels.Pages;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

namespace Nexus.Views;

public partial class MainView : UserControl, ISingletonDependency
{
    public MainView(MainViewModel viewModel, IEnumerable<PageViewModel> pageViewModels)
    {
        DataContext = viewModel;
        InitializeComponent();

        PopulateNavigation(pageViewModels);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (TopLevel.GetTopLevel(this) is FAAppWindow aw)
        {
            // Prior to v3 when AppWindow took care of all the caption button drawing we reported the width
            // of the caption buttons via TitleBar.RightInset. Now we use Avalonia's WindowDrawnDecorations
            // which doesn't report that. However, the default size used in the template I put was
            // to make the buttons 46 dip, so x3 = 138. We'll use 140 for 2 extra padding
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(140, GridUnitType.Pixel);
        }
    }

    private void PopulateNavigation(IEnumerable<PageViewModel> pageViewModels)
    {
        var itemsByFooter = pageViewModels.OrderBy(x => x.Order).ToLookup(page => page.IsFooter);
        NavigationPane.Items.AddRange(itemsByFooter[false].Select(CreateNavigationItem));
        NavigationPane.FooterItems.AddRange(itemsByFooter[true].Select(CreateNavigationItem));
    }

    private static NavigationPaneItem CreateNavigationItem(PageViewModel pageViewModel) =>
        new()
        {
            Title = pageViewModel.Name,
            Icon = new LucideIcon { Kind = pageViewModel.IconKind },
            Page = ProxyHelper.GetUnProxiedType(pageViewModel),
        };
}
