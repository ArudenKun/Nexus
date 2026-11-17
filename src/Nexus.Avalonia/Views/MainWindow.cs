using FluentAvalonia.UI.Windowing;
using Volo.Abp.DependencyInjection;

namespace Nexus.Views;

public class MainWindow : FAAppWindow, ISingletonDependency
{
    public MainWindow()
    {
        Title = NexusConsts.AppName;
        TitleBar.ExtendsContentIntoTitleBar = true;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Content = ViewFactory.Create<ShellView>();
    }
}
