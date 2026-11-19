using Volo.Abp.DependencyInjection;
using Volo.Abp.Ldap;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace Nexus.Identity;

[Dependency(ReplaceServices = true)]
public class LdapSettingDefinitionProvider : Volo.Abp.Ldap.LdapSettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        base.Define(context);

        context.Add(
            new SettingDefinition(
                LdapSettingNames.IsEnabled,
                "false",
                FixedLocalizedString("IsEnabled"),
                FixedLocalizedString("IsEnabled")
            )
        );
    }

    private static ILocalizableString FixedLocalizedString(string name) =>
        new FixedLocalizableString(name);
}
