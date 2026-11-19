using Avalonia.Xaml.Interactions.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Extensions;
using Nexus.Extensions.Controls;
using Nexus.Fluent.Abstractions.Navigation;
using Nexus.Fluent.Abstractions.Views;
using Nexus.Fluent.Extensions;
using Nexus.ViewModels;
using Nexus.ViewModels.Pages;
using Volo.Abp.DependencyInjection;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views;

public class MainView : View<MainViewModel>, ISingletonDependency
{
    private FANavigationView _navView = null!;
    private FAFrame _frameView = null!;

    protected override void OnInitialized()
    {
        _frameView.NavigationPageFactory = new NavigationPageFactory(
            ServiceProvider.GetRequiredService<IViewProvider>()
        );
        ViewModel.PageNavigationService.Navigated += PageNavigationServiceOnNavigated;
        ViewModel.PageNavigationService.NavigateToAsync<HomePageViewModel>().WaitOnDispatcher();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ViewModel.PageNavigationService.Navigated -= PageNavigationServiceOnNavigated;
    }

    private void PageNavigationServiceOnNavigated(object? sender, NavigatedEventArgs e) =>
        _frameView.NavigateFromObject(
            e.Page,
            new FAFrameNavigationOptions { IsNavigationStackEnabled = true }
        );

    protected override object Build(MainViewModel vm) =>
        BusyArea()
            .IsBusy(vm, x => x.IsBusy, BindingMode.OneWay)
            .BusyText(vm, x => x.BusyText, BindingMode.OneWay)
            .Content(
                FANavigationView()
                    .AlwaysShowHeader(true)
                    .PaneDisplayMode(FANavigationViewPaneDisplayMode.Left)
                    .Name("NavView", Scope)
                    .Ref(out _navView)
                    .MenuItemsSource(vm, x => x.MenuPages)
                    .FooterMenuItemsSource(vm, x => x.FooterMenuPages)
                    .AddBehavior(
                        new EventTriggerBehavior()
                            .EventName("SelectionChanged")
                            .Actions([
                                new InvokeCommandAction()
                                    .Command(vm, x => x.ChangePageEventCommand)
                                    .PassEventArgsToCommand(true),
                            ])
                    )
                    .IsSettingsVisible(true)
                    .IsBackButtonVisible(true)
                    .IsBackEnabled(false)
                    .OnBackRequested(_ => _frameView.GoBack())
                    .Content(
                        new Border()
                            .BorderBrush("CardStrokeColorDefaultBrush".GetDynamicResource())
                            .BorderThickness(1)
                            .CornerRadius(8)
                            .Child(
                                new FAFrame()
                                    .Name("FrameView", Scope)
                                    .Ref(out _frameView)
                                    .OnNavigated(
                                        (_, _) => _navView.IsBackEnabled = _frameView.CanGoBack
                                    )
                            )
                    )
            );

    private class NavigationPageFactory : IFANavigationPageFactory
    {
        private readonly IViewProvider _viewProvider;

        public NavigationPageFactory(IViewProvider viewProvider) => _viewProvider = viewProvider;

        public Control GetPage(Type srcType) => throw new NotImplementedException();

        public Control GetPageFromObject(object target) => (Control)_viewProvider.GetView(target);
    }
}
