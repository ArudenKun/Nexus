using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

namespace Nexus.Blazor;

public class NexusStyleBundleContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        context.Files.Add(new BundleFile("main.css", true));
        context.Files.Add("/libs/bootstrap-icons/css/bootstrap-icons.css");
    }
}
