using Avalonia.Controls;
using Fluently.Abstractions.Views;
using Fluently.Views;
using Microsoft.Extensions.DependencyInjection;
using ServiceScan.SourceGenerator;

namespace Nexus.Views;

public static partial class ViewRegistryConfigurator
{
    [ScanForTypes(AssignableTo = typeof(IViewFor<>), Handler = nameof(RegisterHandler))]
    public static partial void Register();

    private static void RegisterHandler<TView, TViewModel>()
        where TView : Control
        where TViewModel : class =>
        ViewRegistry.Add<TViewModel>(
            (sp, viewModel) =>
            {
                var view = sp.GetRequiredService<TView>();
                view.DataContext = viewModel;
                return (IViewFor<TViewModel>)view;
            }
        );
}
