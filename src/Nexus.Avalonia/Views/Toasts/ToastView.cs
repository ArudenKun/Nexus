using Avalonia.Automation;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using Lucide.Avalonia;
using Nexus.Extensions.Controls;
using Nexus.Fluent.Abstractions.Toasts;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views.Toasts;

public class ToastView : View<Toast>
{
    private readonly IToastService _toastService;
    private Border _card = null!;

    public ToastView(IToastService toastService)
    {
        _toastService = toastService;
    }

    protected override void OnInitialized()
    {
        _card.Classes.Add(ViewModel.Severity.ToString().ToLowerInvariant());
    }

    protected override StyleGroup BuildStyles() =>
        [
            new Style<Border>(s => s.Class("toast"))
                .Setter(
                    Avalonia.Controls.Border.BackgroundProperty,
                    new DynamicResourceExtension("FlyoutPresenterBackground")
                )
                .Setter(
                    Avalonia.Controls.Border.BorderBrushProperty,
                    new DynamicResourceExtension("FlyoutBorderThemeBrush")
                )
                .Setter(Avalonia.Controls.Border.BorderThicknessProperty, new Thickness(1))
                .Setter(Avalonia.Controls.Border.CornerRadiusProperty, new CornerRadius(8))
                .Setter(Avalonia.Controls.Border.PaddingProperty, new Thickness(16, 12))
                .Setter(
                    Avalonia.Controls.Border.BoxShadowProperty,
                    BoxShadows.Parse("0 8 24 0 #33000000")
                ),
            new Style<Border>(s => s.Class("toast-tint")).Setter(
                Avalonia.Controls.Border.CornerRadiusProperty,
                new CornerRadius(7)
            ),
            new Style<Border>(s =>
                s.Class("toast")
                    .Class("information")
                    .Descendant()
                    .OfType<Border>()
                    .Class("toast-tint")
            ).Setter(
                Avalonia.Controls.Border.BackgroundProperty,
                new DynamicResourceExtension("SystemFillColorAttentionBackgroundBrush")
            ),
            new Style<Border>(s =>
                s.Class("toast").Class("success").Descendant().OfType<Border>().Class("toast-tint")
            ).Setter(
                Avalonia.Controls.Border.BackgroundProperty,
                new DynamicResourceExtension("SystemFillColorSuccessBackgroundBrush")
            ),
            new Style<Border>(s =>
                s.Class("toast").Class("warning").Descendant().OfType<Border>().Class("toast-tint")
            ).Setter(
                Avalonia.Controls.Border.BackgroundProperty,
                new DynamicResourceExtension("SystemFillColorCautionBackgroundBrush")
            ),
            new Style<Border>(s =>
                s.Class("toast").Class("error").Descendant().OfType<Border>().Class("toast-tint")
            ).Setter(
                Avalonia.Controls.Border.BackgroundProperty,
                new DynamicResourceExtension("SystemFillColorCriticalBackgroundBrush")
            ),
        ];

    protected override object Build(Toast vm) =>
        Border()
            .Ref(out _card)
            .Name("Card", Scope)
            .Classes("toast")
            .Width(380)
            .Padding(0)
            .Background("CardBackgroundFillColorDefaultBrush".GetDynamicResource())
            .BorderBrush("CardStrokeColorDefaultBrush".GetDynamicResource())
            .BorderThickness(1)
            .CornerRadius("OverlayCornerRadius".GetDynamicResource())
            .ClipToBounds(true)
            .BoxShadow(BoxShadows.Parse("0 8 16 0 #24000000"))
            .Child(
                Panel()
                    .Children(
                        Border().Classes("toast-tint"),
                        Grid()
                            .Cols("Auto,*,Auto")
                            .Margin(16, 14, 8, 16)
                            .Children(
                                new LucideIcon().Kind(ToIconKind(vm.Severity)).Margin(0, 2, 14, 0),
                                StackPanel()
                                    .Col(1)
                                    .Spacing(6)
                                    .Margin(0, 0, 12, 0)
                                    .Children(
                                        TextBlock()
                                            .Text(vm, x => x.Title)
                                            .FontWeight(FontWeight.SemiBold)
                                            .TextWrapping(TextWrapping.Wrap),
                                        TextBlock()
                                            .Text(vm, x => x.Message)
                                            .Classes("toast-message")
                                    ),
                                Button()
                                    .Col(2)
                                    .Classes("toast-close")
                                    .OnClick(_ => _toastService.Dismiss(vm))
                                    .ToolTip_Tip("Close")
                                    .With(x => AutomationProperties.SetName(x, "Close"))
                                    .Content(
                                        PathIcon()
                                            .Width(10)
                                            .Height(10)
                                            .Data(
                                                Geometry.Parse(
                                                    "M2.59,1.17L8,6.59L13.41,1.17L14.83,2.59L9.41,8L14.83,13.41L13.41,14.83L8,9.41L2.59,14.83L1.17,13.41L6.59,8L1.17,2.59Z"
                                                )
                                            )
                                    )
                            )
                    )
            );

    private static LucideIconKind ToIconKind(ToastSeverity severity)
    {
        return severity switch
        {
            ToastSeverity.Information => LucideIconKind.Info,
            ToastSeverity.Success => LucideIconKind.Check,
            ToastSeverity.Warning => LucideIconKind.CircleExclamationPoint,
            ToastSeverity.Error => LucideIconKind.ShieldX,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null),
        };
    }
}
