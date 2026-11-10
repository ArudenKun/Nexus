using System;

namespace Nexus.Events;

public sealed record SetGlobalBusyEvent
{
    public SetGlobalBusyEvent(string message, TimeSpan? duration = null)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Duration = duration;
    }

    public SetGlobalBusyEvent(TimeSpan duration, string message)
        : this(message, duration) { }

    public string Message { get; }
    public TimeSpan? Duration { get; }
}
