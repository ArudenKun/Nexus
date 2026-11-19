using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Input;
using Avalonia.Media.Transformation;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using Nexus.Extensions.Controls;
using Nexus.ViewModels;
using Volo.Abp.DependencyInjection;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views;

public class LoginView : View<LoginViewModel>, ISingletonDependency
{
    private Border _tabIndicator = null!;
    private Button _browserTabButton = null!;
    private Button _credentialsTabButton = null!;
    private Panel _authPanel = null!;
    private Grid _browserPanel = null!;
    private Button _browserLoginButton = null!;
    private Grid _credentialsPanel = null!;
    private TextBox _usernameBox = null!;
    private TextBox _passwordBox = null!;
    private PathIcon _passwordEyeIcon = null!;
    private bool _credentialsVisible;
    private bool _passwordVisible;
    private CancellationTokenSource? _toggleCts;

    private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(220);

    private static readonly Geometry EyeOpenGeometry = Geometry.Parse(
        "M12 5C7 5 2.73 8.11 1 12.5C2.73 16.89 7 20 12 20C17 20 21.27 16.89 23 12.5C21.27 8.11 17 5 12 5ZM12 17.5C9.24 17.5 7 15.26 7 12.5C7 9.74 9.24 7.5 12 7.5C14.76 7.5 17 9.74 17 12.5C17 15.26 14.76 17.5 12 17.5ZM12 9.5C10.34 9.5 9 10.84 9 12.5C9 14.16 10.34 15.5 12 15.5C13.66 15.5 15 14.16 15 12.5C15 10.84 13.66 9.5 12 9.5Z"
    );

    private static readonly Geometry EyeOffGeometry = Geometry.Parse(
        "M12 7.5C14.76 7.5 17 9.74 17 12.5C17 13.15 16.87 13.76 16.64 14.33L19.56 17.25C21.07 15.99 22.26 14.36 23 12.5C21.27 8.11 17 5 12 5C10.6 5 9.26 5.25 8.02 5.7L10.18 7.86C10.74 7.63 11.35 7.5 12 7.5ZM2 4.27L4.28 6.55L4.74 7.01C3.08 8.3 1.78 10.22 1 12.5C2.73 16.89 7 20 12 20C13.55 20 15.03 19.7 16.38 19.16L16.8 19.58L19.73 22.5L21 21.23L3.27 3L2 4.27ZM7.53 9.8L9.08 11.35C9.03 11.72 9 12.11 9 12.5C9 14.16 10.34 15.5 12 15.5C12.39 15.5 12.78 15.47 13.15 15.42L14.7 16.97C13.87 17.31 12.96 17.5 12 17.5C9.24 17.5 7 15.26 7 12.5C7 11.54 7.19 10.63 7.53 9.8ZM11.84 9.52L14.98 12.66L15 12.5C15 10.84 13.66 9.5 12 9.5L11.84 9.52Z"
    );

    private static readonly Geometry GlobeGeometry = Geometry.Parse(
        "M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2ZM11 19.93C7.05 19.44 4 16.08 4 12C4 11.38 4.08 10.79 4.21 10.21L9 15V16C9 17.1 9.9 18 11 18V19.93ZM17.9 17.39C17.64 16.58 16.9 16 16 16H15V13C15 12.45 14.55 12 14 12H8V10H10C10.55 10 11 9.55 11 9V7H13C14.1 7 15 6.1 15 5V4.59C17.93 5.78 20 8.65 20 12C20 14.08 19.2 15.97 17.9 17.39Z"
    );

    protected override object Build(LoginViewModel vm) =>
        Border()
            .VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Background("SystemControlBackgroundAltHighBrush".GetDynamicResource())
            .BorderBrush("SystemControlForegroundBaseMediumLowBrush".GetDynamicResource())
            .BorderThickness(1)
            .CornerRadius(10)
            .Padding(32)
            .BoxShadow(BoxShadow.Parse("0 8 24 0 #33000000"))
            .Child(
                StackPanel()
                    .Spacing(16)
                    .Width(320)
                    .Children(
                        #region Header

                        StackPanel()
                            .Spacing(10)
                            .Children(
                                Image()
                                    .Source(
                                        new SvgImage().Source(
                                            SvgSource.Load(
                                                "avares://Nexus.Avalonia/Assets/logo_2.svg"
                                            )
                                        )
                                    )
                                    .Width(52)
                                    .Height(52)
                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                    .Stretch(Stretch.Uniform),
                                StackPanel()
                                    .Spacing(2)
                                    .Children(
                                        TextBlock()
                                            .Text("Welcome Back")
                                            .FontSize(22)
                                            .FontWeight(FontWeight.SemiBold)
                                            .HorizontalAlignment(HorizontalAlignment.Center),
                                        TextBlock()
                                            .Text("Sign in to continue")
                                            .FontSize(13)
                                            .Opacity(0.6)
                                            .HorizontalAlignment(HorizontalAlignment.Center)
                                    )
                            ),
                        #endregion

                        #region Symmetrical Mode Switcher

                        Border()
                            .Background("SystemControlBackgroundBaseLowBrush".GetDynamicResource())
                            .BorderBrush("SystemControlForegroundBaseLowBrush".GetDynamicResource())
                            .BorderThickness(1)
                            .CornerRadius(6)
                            .Padding(3)
                            .Child(
                                Grid()
                                    .Cols("*,*")
                                    .Children(
                                        Border()
                                            .Ref(out _tabIndicator)
                                            .Col(0)
                                            .Background(
                                                "SystemControlBackgroundAltHighBrush".GetDynamicResource()
                                            )
                                            .BorderBrush(
                                                "SystemControlForegroundBaseLowBrush".GetDynamicResource()
                                            )
                                            .BorderThickness(1)
                                            .CornerRadius(4)
                                            .BoxShadow(BoxShadow.Parse("0 1 3 0 #22000000"))
                                            .RenderTransform(
                                                TransformOperations.Parse("translateX(0px)")
                                            )
                                            .Transitions([
                                                new TransformOperationsTransition
                                                {
                                                    Property = RenderTransformProperty,
                                                    Duration = TransitionDuration,
                                                    Easing = new CubicEaseOut(),
                                                },
                                            ]),
                                        Button()
                                            .Ref(out _browserTabButton)
                                            .Col(0)
                                            .Content("Browser")
                                            .FontSize(12)
                                            .FontWeight(FontWeight.SemiBold)
                                            .Opacity(1)
                                            .Padding(0, 6)
                                            .Background(Brushes.Transparent)
                                            .BorderThickness(0)
                                            .CornerRadius(4)
                                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                                            .HorizontalContentAlignment(HorizontalAlignment.Center)
                                            .Transitions(CreateTabOpacityTransitions())
                                            .OnClick(__ =>
                                                _ = SetAuthModeAsync(showCredentials: false)
                                            ),
                                        Button()
                                            .Ref(out _credentialsTabButton)
                                            .Col(1)
                                            .Content("Credentials")
                                            .FontSize(12)
                                            .FontWeight(FontWeight.SemiBold)
                                            .Opacity(0.55)
                                            .Padding(0, 6)
                                            .Background(Brushes.Transparent)
                                            .BorderThickness(0)
                                            .CornerRadius(4)
                                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                                            .HorizontalContentAlignment(HorizontalAlignment.Center)
                                            .Transitions(CreateTabOpacityTransitions())
                                            .OnClick(__ =>
                                                _ = SetAuthModeAsync(showCredentials: true)
                                            )
                                    )
                            ),
                        #endregion

                        #region Symmetrical Animated Auth Container

                        Panel()
                            .Ref(out _authPanel)
                            .ClipToBounds(true)
                            .OnAttachedToVisualTree(_ =>
                            {
                                _credentialsPanel.IsVisible = true;
                                _credentialsPanel.Measure(new Size(320, double.PositiveInfinity));
                                if (_credentialsPanel.DesiredSize.Height > 0)
                                    _authPanel.Height = _credentialsPanel.DesiredSize.Height;
                                _credentialsPanel.IsVisible = false;
                            })
                            .Children(
                                #region Browser SSO Panel

                                Grid()
                                    .Ref(out _browserPanel)
                                    .Rows("*,Auto")
                                    .Opacity(1)
                                    .RenderTransform(TransformOperations.Parse("translateX(0px)"))
                                    .Transitions(CreatePanelTransitions())
                                    .Children(
                                        Border()
                                            .Row(0)
                                            .Margin(0, 0, 0, 14)
                                            .Padding(16, 14)
                                            .CornerRadius(6)
                                            .Background(
                                                "SystemControlBackgroundBaseLowBrush".GetDynamicResource()
                                            )
                                            .BorderBrush(
                                                "SystemControlForegroundBaseLowBrush".GetDynamicResource()
                                            )
                                            .BorderThickness(1)
                                            .Child(
                                                StackPanel()
                                                    .Spacing(8)
                                                    .VerticalAlignment(VerticalAlignment.Center)
                                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                                    .Children(
                                                        PathIcon()
                                                            .Data(GlobeGeometry)
                                                            .Width(22)
                                                            .Height(22)
                                                            .Opacity(0.75)
                                                            .HorizontalAlignment(
                                                                HorizontalAlignment.Center
                                                            ),
                                                        TextBlock()
                                                            .Text(
                                                                "Sign in securely through your organization's single sign-on portal in your browser."
                                                            )
                                                            .FontSize(12.5)
                                                            .Opacity(0.75)
                                                            .TextWrapping(TextWrapping.Wrap)
                                                            .TextAlignment(TextAlignment.Center)
                                                    )
                                            ),
                                        Button()
                                            .Row(1)
                                            .Ref(out _browserLoginButton)
                                            .Name("LoginBrowserButton", Scope)
                                            .Content("Continue with Browser")
                                            .Classes("accent")
                                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                                            .HorizontalContentAlignment(HorizontalAlignment.Center)
                                            .Command(vm, x => x.LoginWithBrowserCommand)
                                    ),
                                #endregion

                                #region Username / Password Panel

                                Grid()
                                    .Name("CredentialsPanel", Scope)
                                    .Ref(out _credentialsPanel)
                                    .Rows("*,Auto")
                                    .IsVisible(false)
                                    .Opacity(0)
                                    .RenderTransform(TransformOperations.Parse("translateX(16px)"))
                                    .Transitions(CreatePanelTransitions())
                                    .Children(
                                        StackPanel()
                                            .Row(0)
                                            .Spacing(12)
                                            .Margin(0, 0, 0, 14)
                                            .VerticalAlignment(VerticalAlignment.Center)
                                            .Children(
                                                StackPanel()
                                                    .Spacing(5)
                                                    .Children(
                                                        TextBlock()
                                                            .Text("User Id")
                                                            .FontSize(12)
                                                            .FontWeight(FontWeight.SemiBold)
                                                            .Opacity(0.85),
                                                        TextBox()
                                                            .Ref(out _usernameBox)
                                                            .Name("UsernameBox", Scope)
                                                            .PlaceholderText("Enter your User Id")
                                                            .Text(vm, x => x.UserId)
                                                            .HorizontalAlignment(
                                                                HorizontalAlignment.Stretch
                                                            )
                                                    ),
                                                StackPanel()
                                                    .Spacing(5)
                                                    .Children(
                                                        TextBlock()
                                                            .Text("Password")
                                                            .FontSize(12)
                                                            .FontWeight(FontWeight.SemiBold)
                                                            .Opacity(0.85),
                                                        TextBox()
                                                            .Ref(out _passwordBox)
                                                            .Name("PasswordBox", Scope)
                                                            .PlaceholderText("Enter your Password")
                                                            .PasswordChar('*')
                                                            .Text(vm, x => x.Password)
                                                            .HorizontalAlignment(
                                                                HorizontalAlignment.Stretch
                                                            )
                                                            .InnerRightContent(
                                                                Button()
                                                                    .Name(
                                                                        "TogglePasswordButton",
                                                                        Scope
                                                                    )
                                                                    .IsTabStop(false)
                                                                    .Background(Brushes.Transparent)
                                                                    .BorderThickness(0)
                                                                    .Padding(6, 4)
                                                                    .Margin(0, 0, 2, 0)
                                                                    .VerticalAlignment(
                                                                        VerticalAlignment.Center
                                                                    )
                                                                    .Content(
                                                                        PathIcon()
                                                                            .Ref(
                                                                                out _passwordEyeIcon
                                                                            )
                                                                            .Data(EyeOpenGeometry)
                                                                            .Width(15)
                                                                            .Height(15)
                                                                            .Opacity(0.7)
                                                                    )
                                                                    .OnClick(_ =>
                                                                        TogglePasswordVisibility()
                                                                    )
                                                            )
                                                    )
                                            ),
                                        Button()
                                            .Row(1)
                                            .Name("CredentialsLoginButton", Scope)
                                            .Content("Sign In")
                                            .Classes("accent")
                                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                                            .HorizontalContentAlignment(HorizontalAlignment.Center)
                                            .Command(vm, x => x.LoginWithCredentialsCommand)
                                    )
                                #endregion
                            )
                        #endregion
                    )
            );

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled || e.Key is not Key.Enter || ViewModel is not { } vm)
            return;

        var command = _credentialsVisible
            ? vm.LoginWithCredentialsCommand
            : vm.LoginWithBrowserCommand;

        if (!command.CanExecute(null))
            return;

        command.Execute(null);
        e.Handled = true;
    }

    private static Transitions CreateTabOpacityTransitions() =>
        [
            new DoubleTransition
            {
                Property = OpacityProperty,
                Duration = TransitionDuration,
                Easing = new CubicEaseOut(),
            },
        ];

    private static Transitions CreatePanelTransitions() =>
        [
            new DoubleTransition
            {
                Property = OpacityProperty,
                Duration = TransitionDuration,
                Easing = new CubicEaseOut(),
            },
            new TransformOperationsTransition
            {
                Property = RenderTransformProperty,
                Duration = TransitionDuration,
                Easing = new CubicEaseOut(),
            },
        ];

    private void TogglePasswordVisibility()
    {
        _passwordVisible = !_passwordVisible;
        _passwordBox.PasswordChar = _passwordVisible ? '\0' : '*';
        _passwordEyeIcon.Data = _passwordVisible ? EyeOffGeometry : EyeOpenGeometry;
    }

    private async Task SetAuthModeAsync(bool showCredentials)
    {
        if (_credentialsVisible == showCredentials)
            return;

        _toggleCts?.Cancel();
        _toggleCts = new CancellationTokenSource();
        var token = _toggleCts.Token;

        _credentialsVisible = showCredentials;

        if (!_credentialsVisible && _passwordVisible)
        {
            _passwordVisible = false;
            _passwordBox.PasswordChar = '*';
            _passwordEyeIcon.Data = EyeOpenGeometry;
        }

        double tabWidth = _browserTabButton.Bounds.Width > 0 ? _browserTabButton.Bounds.Width : 156;
        _tabIndicator.RenderTransform = TransformOperations.Parse(
            _credentialsVisible ? $"translateX({tabWidth}px)" : "translateX(0px)"
        );
        _browserTabButton.Opacity = _credentialsVisible ? 0.55 : 1;
        _credentialsTabButton.Opacity = _credentialsVisible ? 1 : 0.55;

        Panel incoming = _credentialsVisible ? _credentialsPanel : _browserPanel;
        Panel outgoing = _credentialsVisible ? _browserPanel : _credentialsPanel;
        double enterOffset = _credentialsVisible ? 16 : -16;
        double exitOffset = _credentialsVisible ? -16 : 16;

        incoming.Opacity = 0;
        incoming.RenderTransform = TransformOperations.Parse($"translateX({enterOffset}px)");
        incoming.IsVisible = true;
        outgoing.IsVisible = true;

        var frameCommitted = new TaskCompletionSource();
        Dispatcher.UIThread.Post(frameCommitted.SetResult, DispatcherPriority.Render);
        await frameCommitted.Task;
        if (token.IsCancellationRequested)
            return;

        if (_credentialsVisible)
            _usernameBox.Focus();
        else
            _browserLoginButton.Focus();

        incoming.Opacity = 1;
        incoming.RenderTransform = TransformOperations.Parse("translateX(0px)");

        outgoing.Opacity = 0;
        outgoing.RenderTransform = TransformOperations.Parse($"translateX({exitOffset}px)");

        try
        {
            await Task.Delay(TransitionDuration, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        outgoing.IsVisible = false;
    }
}
