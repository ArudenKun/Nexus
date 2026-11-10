using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Configuration.Writable;
using Fluently;
using Microsoft.Extensions.Logging;
using Nexus.Settings;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Nexus.ViewModels;

public interface IViewModel : IUnitOfWorkEnabled, IHasExtraProperties { }

public abstract partial class ViewModel : ObservableObject, IViewModel, ITransientDependency
{
    protected ViewModel()
    {
        ExtraProperties = new ExtraPropertyDictionary();
        this.SetDefaultsForExtraProperties();
    }

    public required IAbpLazyServiceProvider LazyServiceProvider { protected get; init; }

    protected IServiceProvider ServiceProvider =>
        LazyServiceProvider.LazyGetRequiredService<IServiceProvider>();

    protected ILoggerFactory LoggerFactory =>
        LazyServiceProvider.LazyGetRequiredService<ILoggerFactory>();

    protected ILogger Logger =>
        LazyServiceProvider.LazyGetService<ILogger>(_ =>
            LoggerFactory.CreateLogger(GetType().FullName!)
        );

    protected IUnitOfWorkManager UnitOfWorkManager =>
        LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();

    protected IClock Clock => LazyServiceProvider.LazyGetRequiredService<IClock>();

    protected IGuidGenerator GuidGenerator =>
        LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>();

    protected ILocalEventBus LocalEventBus =>
        LazyServiceProvider.LazyGetRequiredService<ILocalEventBus>();

    protected IWritableOptions<ApplicationSettings> ApplicationOptions =>
        LazyServiceProvider.LazyGetRequiredService<IWritableOptions<ApplicationSettings>>();

    public INavigationService NavigationService =>
        LazyServiceProvider.LazyGetRequiredService<INavigationService>();

    public IDialogService DialogService =>
        LazyServiceProvider.LazyGetRequiredService<IDialogService>();

    public IToastService ToastService =>
        LazyServiceProvider.LazyGetRequiredService<IToastService>();

    public ExtraPropertyDictionary ExtraProperties { get; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string BusyText { get; set; } = string.Empty;

    protected virtual async Task SetBusyAsync(
        Func<Task> func,
        string busyText = "",
        bool showException = true
    )
    {
        IsBusy = true;
        BusyText = busyText;
        try
        {
            await func();
        }
        catch (Exception ex) when (LogException(ex, true, showException))
        {
            // Not Used
        }
        finally
        {
            IsBusy = false;
            BusyText = string.Empty;
        }
    }

    protected bool LogException(Exception? ex, bool shouldCatch = false, bool shouldDisplay = false)
    {
        if (ex is null)
            return shouldCatch;

        Logger.LogException(ex);
        if (shouldDisplay)
        {
            ToastService.ShowError("Error", ex.Message);
            //GlobalExceptionHandler.Show(ex);
        }

        return shouldCatch;
    }

    protected virtual void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);
}
