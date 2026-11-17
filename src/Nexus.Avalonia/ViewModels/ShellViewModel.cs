using AsyncAwaitBestPractices;
using Microsoft.Extensions.Logging;
using Nexus.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;

namespace Nexus.ViewModels;

public class ShellViewModel : ViewModel, IDisposable, ISingletonDependency
{
    private bool _suppressBroadcast;

    private CancellationTokenSource? _remoteBusyCts;
    private readonly IDisposable _disposable;

    public ShellViewModel(ILocalEventBus localEventBus)
    {
        _disposable = localEventBus.Subscribe(
            new ActionEventHandler<BusyShellEvent>(OnSetShellBusyAsync)
        );
    }

    private Task OnSetShellBusyAsync(BusyShellEvent eventData)
    {
        // Don't fight our own broadcasts (rare race when a single VM is the driver).
        if (_suppressBroadcast)
            return Task.CompletedTask;

        _remoteBusyCts?.Cancel();
        _remoteBusyCts?.Dispose();
        _remoteBusyCts = null;

        _suppressBroadcast = true;
        try
        {
            IsBusy = eventData.IsBusy;
            BusyText = eventData.IsBusy ? eventData.Message ?? string.Empty : string.Empty;
        }
        finally
        {
            _suppressBroadcast = false;
        }

        if (eventData is { IsBusy: true, Duration: { } duration })
        {
            _remoteBusyCts = new CancellationTokenSource();
            AutoHideRemoteBusyAsync(duration, _remoteBusyCts.Token)
                .SafeFireAndForget(ex => Logger.LogError(ex, "An error occured"));
        }

        return Task.CompletedTask;
    }

    private async Task AutoHideRemoteBusyAsync(TimeSpan duration, CancellationToken token)
    {
        try
        {
            await Task.Delay(duration, token);
            _suppressBroadcast = true;
            try
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
            finally
            {
                _suppressBroadcast = false;
            }
        }
        catch (TaskCanceledException)
        {
            // Superseded by a newer message; nothing to do
        }
    }

    public void Dispose()
    {
        _remoteBusyCts?.Cancel();
        _remoteBusyCts?.Dispose();
        _disposable.Dispose();
    }
}
