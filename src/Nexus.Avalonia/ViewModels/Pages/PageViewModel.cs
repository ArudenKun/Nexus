using Fluently;
using Humanizer;
using Lucide.Avalonia;
using Volo.Abp.DynamicProxy;

namespace Nexus.ViewModels.Pages;

public abstract class PageViewModel : ViewModel, INavigableViewModel
{
    protected PageViewModel()
    {
        var type = ProxyHelper.GetUnProxiedType(this);
        Name = type.Name.Replace("PageViewModel", string.Empty).Humanize(LetterCasing.Title);
    }

    /// <summary>
    /// The Order of the page.
    /// </summary>
    public int Order { get; protected set; } = int.MaxValue;

    /// <summary>
    /// The display name of the page.
    /// </summary>
    public string Name { get; protected set; }

    public bool IsFooter { get; protected set; }

    /// <summary>
    /// The icon of the page.
    /// </summary>
    public abstract LucideIconKind IconKind { get; }
}
