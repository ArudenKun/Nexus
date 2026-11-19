using Avalonia.Xaml.Interactions.Responsive;
using Avalonia.Xaml.Interactivity;

namespace Nexus.Extensions.Controls;

public static class BehaviorExtensions
{
    public static TAction PassEventArgsToCommand<TAction>(this TAction action, bool pass)
        where TAction : InvokeCommandActionBase
    {
        action.PassEventArgsToCommand = pass;
        return action;
    }

    public static TControl AddBehavior<TControl>(this TControl control, IBehavior behavior)
        where TControl : Control
    {
        var collection = Interaction.GetBehaviors(control);
        collection.Add((behavior as AvaloniaObject)!);
        return control;
    }

    public static TControl AddBehaviors<TControl>(
        this TControl control,
        params IBehavior[] behaviors
    )
        where TControl : Control
    {
        var collection = Interaction.GetBehaviors(control);
        foreach (var behavior in behaviors)
            collection.Add((behavior as AvaloniaObject)!);
        return control;
    }

    public static TBehavior Setters<TBehavior>(
        this TBehavior behavior,
        params AdaptiveClassSetter[] setters
    )
        where TBehavior : AdaptiveBehavior
    {
        foreach (var adaptiveClassSetter in setters)
        {
            behavior.Setters.Add(adaptiveClassSetter);
        }
        return behavior;
    }
}
