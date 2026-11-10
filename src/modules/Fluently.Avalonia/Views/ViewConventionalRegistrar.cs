using Avalonia.Controls;
using Fluently.Abstractions.Views;
using Volo.Abp.DependencyInjection;

namespace Fluently.Views;

public sealed class ViewConventionalRegistrar : DefaultConventionalRegistrar
{
    protected override bool IsConventionalRegistrationDisabled(Type type) =>
        !type.GetInterfaces()
            .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IViewFor<>))
            && type.IsAssignableTo<Control>()
        || base.IsConventionalRegistrationDisabled(type);

    protected override List<Type> GetExposedServiceTypes(Type type)
    {
        var exposedServiceTypes = base.GetExposedServiceTypes(type);
        var viewInterfaceTypes = type.GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IViewFor<>))
            .ToList();
        return [.. exposedServiceTypes.Union(viewInterfaceTypes).Distinct()];
    }
}
