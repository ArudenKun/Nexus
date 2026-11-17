using Nexus.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Nexus.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class NexusController : AbpControllerBase
{
    protected NexusController()
    {
        LocalizationResource = typeof(NexusResource);
    }
}
