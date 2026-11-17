using Microsoft.Extensions.DependencyInjection;
using Nexus.Fluent.Abstractions.Toasts;

namespace Nexus.Fluent.Services.Toasts;

/// <summary>
///     A shown toast, the cancellation of its pending auto-dismiss, and the scope it was created from, if any.
/// </summary>
internal sealed record ActiveToast(
    IToast Toast,
    CancellationTokenSource Expiry,
    AsyncServiceScope? Scope
);
