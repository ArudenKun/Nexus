using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Fluently.Abstractions.Views;
using Volo.Abp.DependencyInjection;

namespace Nexus.Views;

public class NavigationPageFactory : IFANavigationPageFactory, ISingletonDependency
{
    public NavigationPageFactory(IViewProvider viewProvider)
    {
        ViewProvider = viewProvider;
    }

    protected IViewProvider ViewProvider { get; }

    public Control GetPage(Type srcType) =>
        throw new NotImplementedException("Not implemented only GetPageFromObject is implemented");

    public Control GetPageFromObject(object target) => (Control)ViewProvider.GetView(target);
}
