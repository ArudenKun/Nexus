using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Fluently.Controls.Navigation;
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
