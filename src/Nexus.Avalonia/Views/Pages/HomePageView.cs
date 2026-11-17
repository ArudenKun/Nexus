using Nexus.ViewModels.Pages;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views.Pages;

public class HomePageView : View<HomePageViewModel>
{
    protected override object Build(HomePageViewModel vm) =>
        BusyArea()
            .BusyText(vm, x => x.BusyText)
            .IsBusy(vm, x => x.IsBusy)
            .Content(
                StackPanel()
                    .Spacing(5)
                    .Children(
                        Button().Content("Set Busy").Command(vm, x => x.SetBusyShellCommand),
                        Button()
                            .Content("Manual Expire Token")
                            .Command(vm, x => x.ManualExpireTokenCommand),
                        TextBlock()
                            .Text(vm, x => x.Data)
                            .TextWrapping(TextWrapping.WrapWithOverflow)
                    )
            );
}
