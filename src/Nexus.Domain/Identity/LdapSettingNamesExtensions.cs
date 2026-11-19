using Volo.Abp.Ldap;

namespace Nexus.Identity;

public static class LdapSettingNamesExtensions
{
    extension(LdapSettingNames)
    {
        public static string IsEnabled => "Abp.Ldap.IsEnabled";
    }
}
