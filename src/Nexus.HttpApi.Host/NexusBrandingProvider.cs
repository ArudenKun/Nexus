using Microsoft.Extensions.Localization;
using Nexus.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Nexus;

[Dependency(ReplaceServices = true)]
public class NexusBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<NexusResource> _localizer;

    public NexusBrandingProvider(IStringLocalizer<NexusResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
