using Avalonia.Data.Converters;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using Nexus.Extensions.Controls;
using Nexus.Views.Components.Containers;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views.Components;

public class SettingsRow : ComponentWithContentBase
{
    public static readonly StyledProperty<FASymbol?> IconSymbolProperty = AvaloniaProperty.Register<
        SettingsRow,
        FASymbol?
    >(nameof(IconSymbol));

    public FASymbol? IconSymbol
    {
        get => GetValue(IconSymbolProperty);
        set => SetValue(IconSymbolProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<
        SettingsRow,
        string
    >(nameof(Title), defaultValue: string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> DescriptionProperty = AvaloniaProperty.Register<
        SettingsRow,
        string
    >(nameof(Description), defaultValue: string.Empty);

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    protected override StyleGroup BuildStyles() =>
        [
            new Style<ToggleSwitch>(s => s.OfType<ToggleSwitch>())
                .MinWidth(0)
                .Height(36)
                .HorizontalContentAlignment(HorizontalAlignment.Right),
            new Style<Slider>(s => s.OfType<Slider>()).MinWidth(140),
            new Style<ComboBox>(s => s.OfType<ComboBox>()).MinWidth(140),
            new Style<TextBox>(s => s.OfType<TextBox>()).MinWidth(140),
            new Style<NumericUpDown>(s => s.OfType<NumericUpDown>()).MinWidth(140),
        ];

    protected override object Build() =>
        new Card()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .MinWidth(148)
            .Height(72)
            .CornerRadius("ControlCornerRadius".GetDynamicResource())
            .BorderThickness(1)
            .Padding(16)
            .Content(
                Grid()
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Cols("Auto,Auto,*,Auto")
                    .Children(
                        FASymbolIcon()
                            .Grid_Column(0)
                            .IsVisible(
                                this,
                                x => x.IconSymbol,
                                BindingMode.OneWay,
                                ObjectConverters.IsNotNull
                            )
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Margin(2, 0, 20, 0)
                            .Width(20)
                            .Height(20)
                            .Symbol(this, x => IconSymbol, BindingMode.OneWay),
                        StackPanel()
                            .Grid_Column(1)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Orientation(Orientation.Vertical)
                            .Children(
                                TextBlock()
                                    .IsVisible(
                                        this,
                                        x => x.Title,
                                        BindingMode.OneWay,
                                        StringConverters.IsNotNullOrEmpty
                                    )
                                    .Foreground("SukiText".GetDynamicResource())
                                    .Text(this, x => x.Title, BindingMode.OneWay),
                                TextBlock()
                                    .IsVisible(
                                        this,
                                        x => x.Description,
                                        BindingMode.OneWay,
                                        StringConverters.IsNotNullOrEmpty
                                    )
                                    .FontSize(12)
                                    .Foreground("SukiLowText".GetDynamicResource())
                                    .Text(this, x => x.Description, BindingMode.OneWay)
                            ),
                        ContentControl()
                            .Grid_Column(3)
                            .HorizontalAlignment(HorizontalAlignment.Right)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Margin(12, 0, 0, 0)
                            .HorizontalContentAlignment(HorizontalAlignment.Right)
                            .Content(this, x => x.Content, BindingMode.OneWay)
                    )
            );
}
