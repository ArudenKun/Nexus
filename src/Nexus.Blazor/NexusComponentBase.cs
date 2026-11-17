using Nexus.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Nexus.Blazor;

public abstract class NexusComponentBase : AbpComponentBase
{
    protected NexusComponentBase()
    {
        LocalizationResource = typeof(NexusResource);
    }
}
