using Avalonia.Controls;
using Nexus.Fluent.Abstractions.Views;
using Volo.Abp.DependencyInjection;

namespace Nexus.Fluent.Views;

public sealed class ViewConventionalRegistrar : DefaultConventionalRegistrar
{
    protected override bool IsConventionalRegistrationDisabled(Type type) =>
        (
            !type.IsAssignableTo<Control>()
            && !type.GetInterfaces()
                .Any(x => x.IsGenericType && typeof(IViewFor<>) == x.GetGenericTypeDefinition())
        ) || base.IsConventionalRegistrationDisabled(type);

    protected override List<Type> GetExposedServiceTypes(Type type)
    {
        var exposedServiceTypes = base.GetExposedServiceTypes(type);
        var viewForInterfaceTypes = type.GetInterfaces()
            .Where(x => x.IsGenericType && typeof(IViewFor<>) == x.GetGenericTypeDefinition());
        return [.. exposedServiceTypes.Union(viewForInterfaceTypes).Distinct()];
    }
}
