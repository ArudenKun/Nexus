using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views.Components;

public class TitleBar : ViewBase
{
    public static readonly StyledProperty<Control?> ContentProperty = AvaloniaProperty.Register<
        TitleBar,
        Control?
    >(nameof(Content));

    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    protected override object Build() =>
        Grid()
            .Rows("Auto, *")
            .Children(
                Grid()
                    .Name("TitleBarHost", Scope)
                    .Cols("Auto,Auto,*,Auto")
                    .Background(Brushes.Transparent)
                    .With(x =>
                        WindowDecorationProperties.SetElementRole(
                            x,
                            WindowDecorationsElementRole.TitleBar
                        )
                    )
                    .Children(
                        Image()
                            .Margin(12, 4)
                            .IsHitTestVisible(false)
                            .Source(
                                new SvgImage().Source(
                                    SvgSource.Load(
                                        "/Assets/logo_2.svg",
                                        new Uri("avares://Nexus.Avalonia")
                                    )
                                )
                            )
                            .Width(18)
                            .Height(18)
                            .DockPanel_Dock(Dock.Left)
                            .Name("WindowIcon", Scope)
                            .With(x =>
                                RenderOptions.SetBitmapInterpolationMode(
                                    x,
                                    BitmapInterpolationMode.HighQuality
                                )
                            )
                            .IsVisible(OperatingSystem.IsWindows()),
                        TextBlock()
                            .Text(
                                new ReflectionBinding(nameof(Avalonia.Controls.Window.Title))
                                {
                                    RelativeSource = new RelativeSource
                                    {
                                        Mode = RelativeSourceMode.FindAncestor,
                                        AncestorType = typeof(Window),
                                    },
                                }
                            )
                            .FontSize(12)
                            .IsHitTestVisible(false)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Grid_Column(1)
                            .IsVisible(OperatingSystem.IsWindows())
                    ),
                Decorator()
                    .With(x =>
                        WindowDecorationProperties.SetElementRole(
                            x,
                            WindowDecorationsElementRole.User
                        )
                    )
                    .Row(1)
                    .Child(this, x => x.Content)
            );
}
