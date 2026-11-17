using Avalonia.Metadata;

namespace Nexus.Views.Components;

public abstract class ComponentWithChildrenBase : ViewBase
{
    public static readonly StyledProperty<Avalonia.Controls.Controls> ChildrenProperty =
        AvaloniaProperty.Register<ComponentWithChildrenBase, Avalonia.Controls.Controls>(
            nameof(Children),
            []
        );

    [Content]
    public Avalonia.Controls.Controls Children
    {
        get => GetValue(ChildrenProperty);
        set => SetValue(ChildrenProperty, value);
    }
}

public static class ComponentWithChildrenBaseExtensions
{
    public static TComponent Children<TComponent>(
        this TComponent component,
        params Control[] children
    )
        where TComponent : ComponentWithChildrenBase
    {
        component.Children.AddRange(children);
        return component;
    }
}
