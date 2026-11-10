using Microsoft.Extensions.Logging;

namespace Fluently.Services.Toasts;

internal sealed partial class ToastService
{
    [LoggerMessage(LogLevel.Error, "Disposing the toast {ViewModelType} after it closed failed.")]
    private partial void LogDisposeFailed(Type viewModelType, Exception exception);
}
