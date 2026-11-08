using System;

namespace Nexus.Messages;

public sealed record SetGlobalBusyMessage
{
    public SetGlobalBusyMessage(string message, TimeSpan? duration = null)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Duration = duration;
    }

    public SetGlobalBusyMessage(TimeSpan duration, string message)
        : this(message, duration) { }

    public string Message { get; }
    public TimeSpan? Duration { get; }
}
