using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using Nexus.Localization;

namespace Nexus.Blazor.Client;

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
