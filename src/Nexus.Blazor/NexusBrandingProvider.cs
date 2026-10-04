using Microsoft.Extensions.Localization;
using Nexus.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Nexus.Blazor;

[Dependency(ReplaceServices = true)]
public class NexusBrandingProvider : DefaultBrandingProvider
{
    private readonly IStringLocalizer<NexusResource> _localizer;

    public NexusBrandingProvider(IStringLocalizer<NexusResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
    public override string? LogoUrl => null;
}
