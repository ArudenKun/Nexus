using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.DependencyInjection;

namespace Nexus.ViewModels;

public sealed class ViewModelConventionalRegistrar : DefaultConventionalRegistrar
{
    protected override bool IsConventionalRegistrationDisabled(Type type) =>
        !type.IsAssignableTo<ViewModel>() || base.IsConventionalRegistrationDisabled(type);

    protected override List<Type> GetExposedServiceTypes(Type type)
    {
        var exposedServiceTypes = base.GetExposedServiceTypes(type);
        var viewModelBaseClasses = type.GetBaseClasses(typeof(ViewModel))
            .Where(x => x.IsAssignableTo<ViewModel>());
        return [.. exposedServiceTypes.Union(viewModelBaseClasses).Distinct()];
    }
}
