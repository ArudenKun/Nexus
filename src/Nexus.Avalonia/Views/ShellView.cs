using Avalonia.Animation;
using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Humanizer;
using Nexus.Extensions;
using Nexus.Extensions.Controls;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Fluent.Extensions;
using Nexus.ViewModels;
using Volo.Abp.DependencyInjection;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views;

public partial class ShellView : ViewBase<ShellViewModel>, ISingletonDependency
{
    public ShellView(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    protected INavigationService NavigationService { get; }

    protected override void OnInitialized()
    {
        NavigationService.NavigateToAsync<LoginViewModel>().WaitOnDispatcher();
    }

    protected override object Build(ShellViewModel vm) =>
        TitleBar()
            .Content(
                BusyArea()
                    .With(x =>
                        WindowDecorationProperties.SetElementRole(
                            x,
                            WindowDecorationsElementRole.User
                        )
                    )
                    .IsBusy(vm, x => x.IsBusy, BindingMode.OneWay)
                    .BusyText(vm, x => x.BusyText, BindingMode.OneWay)
                    .Content(
                        ToastHost()
                            .Toasts(vm, x => x.ToastService, BindingMode.OneWay)
                            .Content(
                                DialogHost()
                                    .Dialogs(vm, x => x.DialogService, BindingMode.OneWay)
                                    .DismissOnBackdropClick(true)
                                    .Backdrop("SmokeFillColorDefaultBrush".GetDynamicResource())
                                    .Content(
                                        NavigationHost()
                                            .PageTransition(
                                                new CrossFade().With(x =>
                                                    x.Duration = 150.Milliseconds()
                                                )
                                            )
                                            .Navigation(NavigationService)
                                    )
                            )
                    )
            );
}
