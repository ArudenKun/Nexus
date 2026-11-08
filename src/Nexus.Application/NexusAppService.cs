using Nexus.Localization;
using Volo.Abp.Application.Services;

namespace Nexus;

/* Inherit your application services from this class.
 */
public abstract class NexusAppService : ApplicationService
{
    protected NexusAppService()
    {
        LocalizationResource = typeof(NexusResource);
    }
}
