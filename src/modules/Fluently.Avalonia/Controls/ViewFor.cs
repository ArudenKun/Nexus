using Avalonia.Controls;
using Fluently.Abstractions.Views;
using Volo.Abp.DependencyInjection;

namespace Fluently.Controls;

/// <summary>
///     A <see cref="UserControl" /> that is the view of <typeparamref name="TViewModel" />.
///     It keeps the view model in <see cref="ViewModel" /> and uses it as its <c>DataContext</c>.
///     Derive from it instead of implementing <see cref="IViewFor{TViewModel}" /> yourself:
///     <code>
/// internal partial class ChatView : ViewFor&lt;ChatViewModel&gt;
/// {
///     public ChatView(ChatViewModel viewModel) : base(viewModel) => InitializeComponent();
/// }
/// </code>
/// </summary>
/// <typeparam name="TViewModel">The type of the view model this view shows.</typeparam>
public abstract class ViewFor<TViewModel> : UserControl, IViewFor<TViewModel>, ITransientDependency
    where TViewModel : class
{
    public new TViewModel DataContext
    {
        get =>
            base.DataContext as TViewModel
            ?? throw new InvalidCastException(
                $"DataContext is null or not of the expected type '{typeof(TViewModel).FullName}'."
            );
        set => base.DataContext = value;
    }

    /// <inheritdoc />
    public TViewModel ViewModel => DataContext;
}
