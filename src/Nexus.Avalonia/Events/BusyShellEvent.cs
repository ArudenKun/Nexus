namespace Nexus.Events;

public sealed record BusyShellEvent
{
    public bool IsBusy { get; }
    public TimeSpan? Duration { get; }
    public string? Message { get; }

    public BusyShellEvent(bool isBusy)
        : this(isBusy, duration: null, message: null) { }

    public BusyShellEvent(bool isBusy, string? message)
        : this(isBusy, duration: null, message: message) { }

    public BusyShellEvent(bool isBusy, TimeSpan? duration, string? message = null)
    {
        IsBusy = isBusy;
        Duration = duration;
        Message = message;
    }
}
