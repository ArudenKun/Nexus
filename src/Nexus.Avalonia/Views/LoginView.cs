using Avalonia.Svg.Skia;
using Nexus.Extensions.Controls;
using Nexus.ViewModels;
using Volo.Abp.DependencyInjection;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views;

public class LoginView : View<LoginViewModel>, ISingletonDependency
{
    private Button _loginBrowserButton = null!;
    private Grid _orDivider = null!;
    private StackPanel _credentialsPanel = null!;
    private Button _toggleCredentialsButton = null!;
    private bool _credentialsVisible;

    protected override object Build(LoginViewModel vm) =>
        Border()
            .VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Background("SystemControlBackgroundAltHighBrush".GetDynamicResource())
            .BorderBrush("SystemControlForegroundBaseMediumLowBrush".GetDynamicResource())
            .BorderThickness(1)
            .CornerRadius(8)
            .Padding(40)
            .BoxShadow(BoxShadow.Parse("0 8 24 0 #33000000"))
            .Child(
                StackPanel()
                    .Spacing(20)
                    .Width(320)
                    .Children(
                        #region Logo

                        Image()
                            .Source(
                                new SvgImage().Source(
                                    SvgSource.Load("avares://Nexus.Avalonia/Assets/logo_2.svg")
                                )
                            )
                            .Width(64)
                            .Height(64)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Stretch(Stretch.Uniform),
                        #endregion

                        #region Header

                        StackPanel()
                            .Spacing(4)
                            .Children(
                                TextBlock()
                                    .Text("Welcome Back")
                                    .FontSize(24)
                                    .FontWeight(FontWeight.SemiBold)
                                    .HorizontalAlignment(HorizontalAlignment.Center),
                                TextBlock()
                                    .Text("Sign in to continue")
                                    .FontSize(13)
                                    .Opacity(0.6)
                                    .HorizontalAlignment(HorizontalAlignment.Center)
                            ),
                        #endregion

                        #region Login With Browser

                        Button()
                            .Ref(out _loginBrowserButton)
                            .Name("LoginBrowserButton", Scope)
                            .Content("Login with Browser")
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .HorizontalContentAlignment(HorizontalAlignment.Center)
                            .Classes("accent")
                            .Command(vm, x => x.LoginWithBrowserCommand),
                        #endregion

                        #region Divider with "or"

                        Grid()
                            .Ref(out _orDivider)
                            .Cols("*,Auto,*")
                            .Children(
                                Border()
                                    .Height(1)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Background(
                                        "SystemControlForegroundBaseLowBrush".GetDynamicResource()
                                    ),
                                TextBlock()
                                    .Col(1)
                                    .Text("or")
                                    .Margin(12, 0)
                                    .FontWeight(FontWeight.SemiBold)
                                    .FontSize(14)
                                    .Opacity(0.6)
                                    .VerticalAlignment(VerticalAlignment.Center),
                                Border()
                                    .Col(2)
                                    .Height(1)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Background(
                                        "SystemControlForegroundBaseLowBrush".GetDynamicResource()
                                    )
                            ),
                        #endregion

                        #region Username / Password form (hidden by default)

                        StackPanel()
                            .Name("CredentialsPanel", Scope)
                            .Ref(out _credentialsPanel)
                            .Spacing(12)
                            .IsVisible(false)
                            .Children(
                                TextBox()
                                    .Name("UsernameBox", Scope)
                                    .PlaceholderText("User Id")
                                    .Text(vm, x => x.UserId)
                                    .HorizontalAlignment(HorizontalAlignment.Stretch),
                                TextBox()
                                    .Name("PasswordBox", Scope)
                                    .PlaceholderText("Password")
                                    .PasswordChar('*')
                                    .Text(vm, x => x.Password)
                                    .HorizontalAlignment(HorizontalAlignment.Stretch),
                                Button()
                                    .Name("CredentialsLoginButton", Scope)
                                    .Content("Login")
                                    .Classes("accent")
                                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                                    .HorizontalContentAlignment(HorizontalAlignment.Center)
                                    .Command(vm, x => x.LoginWithCredentialsCommand)
                            ),
                        #endregion

                        #region Toggle link

                        Button()
                            .Ref(out _toggleCredentialsButton)
                            .Name("ToggleCredentialsButton", Scope)
                            .Content("Login with username and password")
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Background(Brushes.Transparent)
                            .BorderThickness(0)
                            .Padding(0)
                            .Foreground("SystemAccentColor".GetDynamicResource())
                            .FontSize(12)
                            .OnClick(_ =>
                            {
                                _credentialsVisible = !_credentialsVisible;
                                _loginBrowserButton.IsVisible = !_credentialsVisible;
                                _orDivider.IsVisible = !_credentialsVisible;
                                _credentialsPanel.IsVisible = _credentialsVisible;
                                _toggleCredentialsButton.Content = _credentialsVisible
                                    ? "Login with Browser Instead"
                                    : "Login with User Id and Password";
                            })
                        #endregion

                    )
            );
}
