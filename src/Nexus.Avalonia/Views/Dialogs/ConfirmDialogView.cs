using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Nexus.ViewModels.Dialogs;
using static Avalonia.Markup.Declarative.Factories;

namespace Nexus.Views.Dialogs;

public class ConfirmDialogView : View<ConfirmDialogViewModel>
{
    protected override StyleGroup BuildStyles() =>
        [
            new Style<Border>(s => s.Class("dialog"))
                .Background(DynamicResource("SolidBackgroundFillColorBaseBrush"))
                .BorderBrush(DynamicResource("SurfaceStrokeColorDefaultBrush"))
                .BorderThickness(1)
                .CornerRadius(8)
                .Padding(24)
                .Width(400)
                .BoxShadow(BoxShadows.Parse("0 8 32 0 #40000000")),
        ];

    protected override object Build(ConfirmDialogViewModel vm) =>
        Border()
            .Classes("dialog")
            .Child(
                StackPanel()
                    .Spacing(24)
                    .Children(
                        StackPanel()
                            .Spacing(12)
                            .Children(
                                TextBlock()
                                    .Text(vm, x => x.Request.Title)
                                    .Classes("subtitle-heading"),
                                TextBlock().Text(vm, x => x.Request.Message).Classes("subtitle")
                            ),
                        new UniformGrid()
                            .Columns(2)
                            .ColumnSpacing(8)
                            .Children(
                                new Button()
                                    .Content(vm, x => x.Request.ConfirmText)
                                    .Command(vm, x => x.ConfirmCommand)
                                    .Classes("accent")
                                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                                    .HorizontalContentAlignment(HorizontalAlignment.Center),
                                new Button()
                                    .Content("Cancel")
                                    .Command(vm, x => x.CancelCommand)
                                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                                    .HorizontalContentAlignment(HorizontalAlignment.Center)
                            )
                    )
            );
}
