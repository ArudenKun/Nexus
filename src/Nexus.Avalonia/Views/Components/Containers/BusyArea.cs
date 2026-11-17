using Avalonia.Animation;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views.Components.Containers;

public class BusyArea : ViewBase
{
    private static readonly IValueConverter ContentOpacityConverter = new FuncValueConverter<
        bool,
        double
    >(value => value ? 0.1 : 1.0);

    private static readonly IValueConverter OverlayOpacityConverter = new FuncValueConverter<
        bool,
        double
    >(value => value ? 1.0 : 0.0);

    private static readonly IValueConverter ProgressContentConverter = new FuncValueConverter<
        bool,
        Control
    >(value =>
    {
        if (value)
        {
            return new FAProgressRing
            {
                IsActive = true,
                Width = 50,
                Height = 50,
            };
        }

        return new Panel();
    });

    public static readonly StyledProperty<bool> IsBusyProperty = AvaloniaProperty.Register<
        BusyArea,
        bool
    >(nameof(IsBusy), defaultValue: false);

    public bool IsBusy
    {
        get => GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly StyledProperty<string?> BusyTextProperty = AvaloniaProperty.Register<
        BusyArea,
        string?
    >(nameof(BusyText), defaultValue: null);

    public string? BusyText
    {
        get => GetValue(BusyTextProperty);
        set => SetValue(BusyTextProperty, value);
    }

    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<
        BusyArea,
        object?
    >(nameof(Content));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    protected override object Build() =>
        Panel()
            .Children(
                ContentControl()
                    .Name("BusyContent")
                    .Content(this, x => x.Content)
                    .Opacity(this, x => x.IsBusy, converter: ContentOpacityConverter)
                    .Transitions([
                        new DoubleTransition()
                            .Property(OpacityProperty)
                            .Duration(TimeSpan.FromSeconds(0.3)),
                    ]),
                DockPanel()
                    .Name("LoadingBusyArea")
                    .LastChildFill(true)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Opacity(this, x => x.IsBusy, converter: OverlayOpacityConverter)
                    .IsHitTestVisible(this, x => x.IsBusy)
                    .Transitions([
                        new DoubleTransition()
                            .Property(OpacityProperty)
                            .Duration(TimeSpan.FromSeconds(0.3)),
                    ])
                    .Children(
                        TextBlock()
                            .DockPanel_Dock(Dock.Bottom)
                            .Name("PART_BusyText")
                            .Margin(3)
                            .FontWeight(FontWeight.SemiBold)
                            .Text(this, x => x.BusyText)
                            .IsVisible(this, x => x.IsBusy),
                        ContentPresenter()
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .IsHitTestVisible(false)
                            .Content(this, x => x.IsBusy, converter: ProgressContentConverter)
                    )
            );
}
