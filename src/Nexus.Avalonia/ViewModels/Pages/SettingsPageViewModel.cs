using Lucide.Avalonia;
using Volo.Abp.DependencyInjection;

namespace Nexus.ViewModels.Pages;

public partial class SettingsPageViewModel : PageViewModel, ISingletonDependency
{
    public SettingsPageViewModel()
    {
        IsFooter = true;
    }

    public override LucideIconKind IconKind => LucideIconKind.Settings;
}
