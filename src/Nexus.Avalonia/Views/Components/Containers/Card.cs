using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views.Components.Containers;

public partial class Card : ComponentWithContentBase
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty = AvaloniaProperty.Register<
        Card,
        IBrush?
    >(nameof(Background));

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly StyledProperty<IBrush?> BorderBrushProperty = AvaloniaProperty.Register<
        Card,
        IBrush?
    >(nameof(BorderBrush));

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<Card, Thickness>(nameof(BorderThickness), new Thickness(1));

    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<Card, CornerRadius>(nameof(CornerRadius));

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Card()
    {
        Bind(
            BackgroundProperty,
            this.GetResourceObservable("CardBackgroundFillColorDefaultBrush"),
            BindingPriority.Style
        );
        Bind(
            BorderBrushProperty,
            this.GetResourceObservable("CardStrokeColorDefaultBrush"),
            BindingPriority.Style
        );
        Bind(
            CornerRadiusProperty,
            this.GetResourceObservable("OverlayCornerRadius"),
            BindingPriority.Style
        );
    }

    protected override object Build() =>
        Border()
            .Background(this, x => x.Background, BindingMode.OneWay)
            .BorderBrush(this, x => x.BorderBrush, BindingMode.OneWay)
            .BorderThickness(this, x => x.BorderThickness, BindingMode.OneWay)
            .CornerRadius(this, x => x.CornerRadius, BindingMode.OneWay)
            .Padding(this, x => x.Padding, BindingMode.OneWay)
            .BackgroundSizing(BackgroundSizing.InnerBorderEdge)
            .Child(this, x => x.Content, BindingMode.OneWay);
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class CardExtensions
{
    public static T CornerRadius<T>(
        this T control,
        BindingBase binding,
        [CallerFilePath] string? _callerFile = null,
        [CallerLineNumber] int _callerLine = 0
    )
        where T : Card =>
        control.BindValue(Card.CornerRadiusProperty, binding, _callerFile, _callerLine);

    public static T BorderThickness<T>(
        this T control,
        double uniformLength,
        [CallerFilePath] string? _callerFile = null,
        [CallerLineNumber] int _callerLine = 0
    )
        where T : Card =>
        control._set(
            () => control.BorderThickness = new Thickness(uniformLength),
            _callerFile,
            _callerLine
        );

    /// <summary>
    /// Gets or sets the thickness of the border.
    /// </summary>
    public static T BorderThickness<T>(
        this T control,
        double horizontal,
        double vertical,
        [CallerFilePath] string? _callerFile = null,
        [CallerLineNumber] int _callerLine = 0
    )
        where T : Card =>
        control._set(
            () => control.BorderThickness = new Thickness(horizontal, vertical),
            _callerFile,
            _callerLine
        );

    /// <summary>
    /// Gets or sets the thickness of the border.
    /// </summary>
    public static T BorderThickness<T>(
        this T control,
        double left,
        double top,
        double right,
        double bottom,
        [CallerFilePath] string? _callerFile = null,
        [CallerLineNumber] int _callerLine = 0
    )
        where T : Card =>
        control._set(
            () => control.BorderThickness = new Thickness(left, top, right, bottom),
            _callerFile,
            _callerLine
        );
}
