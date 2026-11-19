using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nexus.Fluent.Abstractions.Navigation;

namespace Nexus.ViewModels.Dialogs;

public sealed record ConfirmRequest(string Title, string Message, string ConfirmText);

public partial class ConfirmDialogViewModel : DialogViewModel<bool>, INavigatedTo<ConfirmRequest>
{
    [ObservableProperty]
    public partial ConfirmRequest? Request { get; private set; }

    public Task OnNavigatedToAsync(ConfirmRequest parameter, CancellationToken cancellationToken)
    {
        Request = parameter;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task ConfirmAsync() => CloseAsync(true);

    [RelayCommand]
    private Task CancelAsync() => CloseAsync(false);
}
