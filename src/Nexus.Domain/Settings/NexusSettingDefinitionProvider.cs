using Volo.Abp.Settings;

namespace Nexus.Settings;

public class NexusSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(NexusSettings.MySetting1));
    }
}
