using Avalonia.Controls;
using Volo.Abp.DependencyInjection;

namespace Nexus.Views;

public partial class MainSplashScreenContent : UserControl, ISingletonDependency
{
    public MainSplashScreenContent()
    {
        InitializeComponent();
    }
}
