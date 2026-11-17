namespace Nexus.Fluent.Abstractions.Toasts;

/// <summary>
/// A toast that remembers the toast service showing it, so it can close itself: <see cref="ToastViewModel"/>.
/// </summary>
public interface IHostedToast
{
    IToastService? Toasts { set; }
}
