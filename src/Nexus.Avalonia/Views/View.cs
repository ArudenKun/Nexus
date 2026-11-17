using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.Fluent.Abstractions.Views;
using Nexus.ViewModels;
using Volo.Abp.DependencyInjection;

namespace Nexus.Views;

public abstract class View<TViewModel>
    : ViewBase<TViewModel>,
        IViewFor<TViewModel>,
        ITransientDependency
    where TViewModel : class
{
    /// <summary>Gets or sets the view model exposed as this view's data context.</summary>
    public new virtual TViewModel ViewModel =>
        DataContext as TViewModel
        ?? throw new InvalidCastException(
            $"DataContext is null or not of the expected type '{typeof(TViewModel).FullName}'."
        );

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public ITransientCachedServiceProvider LazyServiceProvider { protected get; init; } = null!;

    protected IServiceProvider ServiceProvider =>
        LazyServiceProvider.GetRequiredService<IServiceProvider>();

    protected ILoggerFactory LoggerFactory =>
        LazyServiceProvider.GetRequiredService<ILoggerFactory>();

    protected ILogger Logger =>
        LazyServiceProvider.GetService<ILogger>(_ =>
            LoggerFactory.CreateLogger(GetType().FullName!)
        );
}
