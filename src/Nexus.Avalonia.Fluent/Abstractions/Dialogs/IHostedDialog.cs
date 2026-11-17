namespace Nexus.Fluent.Abstractions.Dialogs;

/// <summary>
/// A dialog that remembers the dialog service showing it, so it can close itself: <see cref="DialogViewModel{TResult}"/>.
/// </summary>
public interface IHostedDialog
{
    IDialogService? Dialogs { set; }
}
