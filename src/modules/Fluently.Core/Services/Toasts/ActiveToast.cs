using Fluently.Abstractions.Toasts;
using Microsoft.Extensions.DependencyInjection;

namespace Fluently.Services.Toasts;

/// <summary>
///     A shown toast, the cancellation of its pending auto-dismiss, and the scope it was created from, if any.
/// </summary>
internal sealed record ActiveToast(
    IToast Toast,
    CancellationTokenSource Expiry,
    AsyncServiceScope? Scope
);
