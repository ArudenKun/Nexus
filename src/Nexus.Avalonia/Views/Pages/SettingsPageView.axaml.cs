using Fluently;
using Nexus.ViewModels.Pages;

namespace Nexus.Views.Pages;

public partial class SettingsPageView : ViewFor<SettingsPageViewModel>
{
    public SettingsPageView(SettingsPageViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
