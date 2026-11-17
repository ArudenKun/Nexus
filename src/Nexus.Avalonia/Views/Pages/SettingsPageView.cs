using Nexus.ViewModels.Pages;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views.Pages;

public class SettingsPageView : View<SettingsPageViewModel>
{
    protected override object Build(SettingsPageViewModel vm) => TextBlock().Text("Settings Page");
}
