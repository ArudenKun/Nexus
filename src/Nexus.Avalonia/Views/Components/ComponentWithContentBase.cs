using Avalonia.Metadata;

namespace Nexus.Views.Components;

public abstract class ComponentWithContentBase : ViewBase
{
    public static readonly StyledProperty<Control?> ContentProperty = AvaloniaProperty.Register<
        ComponentWithContentBase,
        Control?
    >(nameof(Content));

    [Content]
    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}
