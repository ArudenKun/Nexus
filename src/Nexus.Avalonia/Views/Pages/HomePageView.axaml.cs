using Fluently;
using Nexus.ViewModels.Pages;

namespace Nexus.Views.Pages;

public partial class HomePageView : ViewFor<HomePageViewModel>
{
    public HomePageView(HomePageViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
