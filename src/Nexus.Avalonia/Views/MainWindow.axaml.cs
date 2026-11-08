using System;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Views;

public partial class MainWindow : FAAppWindow
{
    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        TitleBar.ExtendsContentIntoTitleBar = true;

        SplashScreen = (IFAApplicationSplashScreen)
            ActivatorUtilities.CreateInstance(serviceProvider, typeof(MainSplashScreen), this);
    }
}
