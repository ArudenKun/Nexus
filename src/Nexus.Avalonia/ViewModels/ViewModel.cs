using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using Configuration.Writable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.Fluent.Abstractions.Dialogs;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Fluent.Abstractions.Toasts;
using Nexus.Fluent.Extensions;
using Nexus.Options;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Nexus.ViewModels;

public abstract partial class ViewModel
    : ObservableObject,
        IUnitOfWorkEnabled,
        IHasExtraProperties,
        ITransientDependency
{
    private CancellationTokenSource? _internalBusyCts;

    protected ViewModel()
    {
        ExtraProperties = new ExtraPropertyDictionary();
        this.SetDefaultsForExtraProperties();
    }

    public required ITransientCachedServiceProvider LazyServiceProvider { protected get; init; }

    protected IServiceProvider ServiceProvider =>
        LazyServiceProvider.GetRequiredService<IServiceProvider>();

    protected ILoggerFactory LoggerFactory =>
        LazyServiceProvider.GetRequiredService<ILoggerFactory>();

    protected ILogger Logger =>
        LazyServiceProvider.GetService<ILogger>(_ =>
            LoggerFactory.CreateLogger(GetType().FullName!)
        );

    protected IUnitOfWorkManager UnitOfWorkManager =>
        LazyServiceProvider.GetRequiredService<IUnitOfWorkManager>();

    protected IClock Clock => LazyServiceProvider.GetRequiredService<IClock>();

    protected IGuidGenerator GuidGenerator =>
        LazyServiceProvider.GetRequiredService<IGuidGenerator>();

    protected ILocalEventBus LocalEventBus =>
        LazyServiceProvider.GetRequiredService<ILocalEventBus>();

    protected IWritableOptions<ApplicationOptions> ApplicationOptions =>
        LazyServiceProvider.GetRequiredService<IWritableOptions<ApplicationOptions>>();

    protected ApplicationOptions CurrentApplicationOptions => ApplicationOptions.CurrentValue;

    public INavigationService PageNavigationService =>
        LazyServiceProvider.GetRequiredKeyedService<INavigationService>("Page");

    public INavigationService NavigationService =>
        LazyServiceProvider.GetRequiredService<INavigationService>();

    public IDialogService DialogService => LazyServiceProvider.GetRequiredService<IDialogService>();

    public IToastService ToastService => LazyServiceProvider.GetRequiredService<IToastService>();

    public ExtraPropertyDictionary ExtraProperties { get; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string BusyText { get; set; } = string.Empty;

    protected virtual void SetBusy(
        Func<Task> func,
        string busyText = "",
        bool showException = true,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string? caller = null
    ) =>
        SetBusyAsync(func, busyText, showException, cancellationToken)
            .SafeFireAndForget(ex =>
                Logger.LogError(
                    ex,
                    "An error occured while in the busy {Type}:{Caller}",
                    ProxyHelper.GetUnProxiedType(GetType()).FullName,
                    caller
                )
            );

    protected virtual void SetBusy(
        Func<CancellationToken, Task> func,
        string busyText = "",
        bool showException = true,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string? caller = null
    ) =>
        SetBusyAsync(func, busyText, showException, cancellationToken)
            .SafeFireAndForget(ex =>
                Logger.LogError(
                    ex,
                    "An error occured while in the busy {Type}:{Caller}",
                    ProxyHelper.GetUnProxiedType(GetType()).FullName,
                    caller
                )
            );

    protected virtual Task SetBusyAsync(
        Func<Task> func,
        string busyText = "",
        bool showException = true,
        CancellationToken cancellationToken = default
    ) => SetBusyAsync(_ => func(), busyText, showException, cancellationToken);

    protected virtual async Task SetBusyAsync(
        Func<CancellationToken, Task> func,
        string busyText = "",
        bool showException = true,
        CancellationToken cancellationToken = default
    )
    {
        var currentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var previousCts = Interlocked.Exchange(ref _internalBusyCts, currentCts);
        previousCts?.Cancel();

        IsBusy = true;
        BusyText = busyText;

        try
        {
            currentCts.Token.ThrowIfCancellationRequested();
            await func(currentCts.Token);
        }
        catch (OperationCanceledException) when (currentCts.IsCancellationRequested)
        {
            // Cancellation was requested — swallow silently
        }
        catch (Exception ex) when (LogException(ex, true, showException))
        {
            // Handled by LogException
        }
        finally
        {
            if (
                ReferenceEquals(
                    Interlocked.CompareExchange(ref _internalBusyCts, null, currentCts),
                    currentCts
                )
            )
            {
                ResetBusyState();
            }

            currentCts.Dispose();
        }
    }

    public virtual void CancelBusy()
    {
        var cts = Interlocked.Exchange(ref _internalBusyCts, null);
        cts?.Cancel();

        ResetBusyState();
    }

    private void ResetBusyState()
    {
        IsBusy = false;
        BusyText = string.Empty;
    }

    protected bool LogException(Exception? ex, bool shouldCatch = false, bool shouldDisplay = false)
    {
        if (ex is null)
            return shouldCatch;

        Logger.LogException(ex);
        if (shouldDisplay)
        {
            ToastService.ShowError("Error", ex.Message);
        }

        return shouldCatch;
    }

    protected virtual void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);
}
