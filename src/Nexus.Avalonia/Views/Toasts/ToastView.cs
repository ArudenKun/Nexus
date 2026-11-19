using Avalonia.Automation;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using Nexus.Fluent.Abstractions.Toasts;
using static Avalonia.Markup.Declarative.Factories;
using TemplatedControl = Avalonia.Controls.Primitives.TemplatedControl;

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
        UpdateSeverityClass();
    }

    private void UpdateSeverityClass()
    {
        _card.Classes.Add(ViewModel.Severity.ToString().ToLowerInvariant());
    }

    protected override StyleGroup BuildStyles() =>
        [
            new Style<Border>(s => s.Class("toast"))
                .Setter(
                    Avalonia.Controls.Border.BackgroundProperty,
                    new DynamicResourceExtension("CardBackgroundFillColorDefaultBrush")
                )
                .Setter(
                    Avalonia.Controls.Border.BorderBrushProperty,
                    new DynamicResourceExtension("CardStrokeColorDefaultBrush")
                )
                .Setter(Avalonia.Controls.Border.BorderThicknessProperty, new Thickness(1))
                .Setter(
                    Avalonia.Controls.Border.CornerRadiusProperty,
                    new DynamicResourceExtension("OverlayCornerRadius")
                )
                .Setter(
                    Avalonia.Controls.Border.BoxShadowProperty,
                    BoxShadows.Parse("0 8 16 0 #24000000")
                ),
            new Style<Border>(s => s.Class("toast-tint")).Setter(
                Avalonia.Controls.Border.CornerRadiusProperty,
                new DynamicResourceExtension("OverlayCornerRadius")
            ),
            new Style<FAFontIcon>(s => s.Class("toast-icon"))
                .Setter(
                    FluentAvalonia.UI.Controls.FAFontIcon.FontFamilyProperty,
                    new DynamicResourceExtension("SymbolThemeFontFamily")
                )
                .Setter(FluentAvalonia.UI.Controls.FAFontIcon.FontSizeProperty, 16d),
            new Style<Button>(s => s.Class("toast-close"))
                .Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent)
                .Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0))
                .Setter(TemplatedControl.PaddingProperty, new Thickness(6))
                .Setter(VerticalAlignmentProperty, VerticalAlignment.Top),
            // Information
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
            new Style<FAFontIcon>(s =>
                s.Class("toast")
                    .Class("information")
                    .Descendant()
                    .OfType<FAFontIcon>()
                    .Class("toast-icon")
            ).Setter(
                TemplatedControl.ForegroundProperty,
                new DynamicResourceExtension("SystemFillColorAttentionBrush")
            ),
            // Success
            new Style<Border>(s =>
                s.Class("toast").Class("success").Descendant().OfType<Border>().Class("toast-tint")
            ).Setter(
                Avalonia.Controls.Border.BackgroundProperty,
                new DynamicResourceExtension("SystemFillColorSuccessBackgroundBrush")
            ),
            new Style<FAFontIcon>(s =>
                s.Class("toast")
                    .Class("success")
                    .Descendant()
                    .OfType<FAFontIcon>()
                    .Class("toast-icon")
            ).Setter(
                TemplatedControl.ForegroundProperty,
                new DynamicResourceExtension("SystemFillColorSuccessBrush")
            ),
            // Warning
            new Style<Border>(s =>
                s.Class("toast").Class("warning").Descendant().OfType<Border>().Class("toast-tint")
            ).Setter(
                Avalonia.Controls.Border.BackgroundProperty,
                new DynamicResourceExtension("SystemFillColorCautionBackgroundBrush")
            ),
            new Style<FAFontIcon>(s =>
                s.Class("toast")
                    .Class("warning")
                    .Descendant()
                    .OfType<FAFontIcon>()
                    .Class("toast-icon")
            ).Setter(
                TemplatedControl.ForegroundProperty,
                new DynamicResourceExtension("SystemFillColorCautionBrush")
            ),
            // Error
            new Style<Border>(s =>
                s.Class("toast").Class("error").Descendant().OfType<Border>().Class("toast-tint")
            ).Setter(
                Avalonia.Controls.Border.BackgroundProperty,
                new DynamicResourceExtension("SystemFillColorCriticalBackgroundBrush")
            ),
            new Style<FAFontIcon>(s =>
                s.Class("toast")
                    .Class("error")
                    .Descendant()
                    .OfType<FAFontIcon>()
                    .Class("toast-icon")
            ).Setter(
                TemplatedControl.ForegroundProperty,
                new DynamicResourceExtension("SystemFillColorCriticalBrush")
            ),
        ];

    protected override object Build(Toast vm) =>
        Border()
            .Ref(out _card)
            .Name("Card", Scope)
            .Classes("toast")
            .Width(380)
            .ClipToBounds(true)
            .With(x => AutomationProperties.SetLiveSetting(x, AutomationLiveSetting.Polite))
            .Child(
                Panel()
                    .Children(
                        Border().Classes("toast-tint"),
                        Grid()
                            .Cols("Auto,*,Auto")
                            .Margin(16, 14, 10, 14)
                            .Children(
                                FAFontIcon()
                                    .Glyph(ToIconGlyph(vm.Severity))
                                    .Classes("toast-icon")
                                    .VerticalAlignment(VerticalAlignment.Top)
                                    .Margin(0, 2, 12, 0),
                                StackPanel()
                                    .Col(1)
                                    .Spacing(4)
                                    .Margin(0, 0, 12, 0)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Children(
                                        TextBlock()
                                            .Text(vm, x => x.Title)
                                            .FontWeight(FontWeight.SemiBold)
                                            .TextWrapping(TextWrapping.Wrap),
                                        TextBlock()
                                            .Text(vm, x => x.Message)
                                            .Classes("toast-message")
                                            .TextWrapping(TextWrapping.Wrap)
                                            .IsVisible(
                                                vm,
                                                x => x.Message,
                                                converter: StringConverters.IsNotNullOrEmpty
                                            )
                                    ),
                                Button()
                                    .Col(2)
                                    .Classes("toast-close")
                                    .OnClick(_ => _toastService.Dismiss(vm))
                                    .ToolTip_Tip("Close")
                                    .With(x => AutomationProperties.SetName(x, "Close"))
                                    .Content(
                                        new FASymbolIcon { Symbol = FASymbol.Cancel, FontSize = 12 }
                                    )
                            )
                    )
            );

    private static string ToIconGlyph(ToastSeverity severity) =>
        severity switch
        {
            ToastSeverity.Information => "\uE946", // Info
            ToastSeverity.Success => "\uEC61", // CompletedSolid (use "\uE73E" for outline CheckMark)
            ToastSeverity.Warning => "\uE7BA", // Warning
            ToastSeverity.Error => "\uEB90", // StatusErrorFull (use "\uEA39" for ErrorBadge)
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null),
        };
}
