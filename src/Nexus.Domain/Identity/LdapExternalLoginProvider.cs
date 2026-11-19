//using System.Threading.Tasks;
//using LdapForNet;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Options;
//using Nexus.Identity;
//using Volo.Abp.DependencyInjection;
//using Volo.Abp.Guids;
//using Volo.Abp.Identity;
//using Volo.Abp.Ldap;
//using Volo.Abp.MultiTenancy;
//using Volo.Abp.Settings;

//namespace Test.Identity;

//public class LdapExternalLoginProvider : ExternalLoginProviderBase, ITransientDependency
//{
//    public const string Name = "Ldap";

//    private readonly ILdapManager _ldapManager;
//    private readonly ILdapSettingProvider _ldapSettingProvider;
//    private readonly ISettingProvider _settingProvider;

//    public LdapExternalLoginProvider(
//        IGuidGenerator guidGenerator,
//        ICurrentTenant currentTenant,
//        IdentityUserManager userManager,
//        IIdentityUserRepository identityUserRepository,
//        IOptions<IdentityOptions> identityOptions,
//        ILdapManager ldapManager,
//        ILdapSettingProvider ldapSettingProvider,
//        ISettingProvider settingProvider
//    )
//        : base(guidGenerator, currentTenant, userManager, identityUserRepository, identityOptions)
//    {
//        _ldapManager = ldapManager;
//        _ldapSettingProvider = ldapSettingProvider;
//        _settingProvider = settingProvider;
//    }

//    public override async Task<bool> IsEnabledAsync() =>
//        await _settingProvider.IsTrueAsync(LdapSettingNames.IsEnabled);

//    public override Task<bool> TryAuthenticateAsync(string userName, string plainPassword) =>
//        _ldapManager.AuthenticateAsync(userName, plainPassword);

//    protected override async Task<ExternalLoginUserInfo> GetUserInfoAsync(string userName)
//    {
//        LdapEntry? entry = null;
//        if (_ldapManager is GlauthLdapManager glauthManager)
//        {
//            entry = await glauthManager.FindUserEntryAsync(userName);
//        }

//        var email = GetAttribute(entry, "mail") ?? $"{userName}@glauth.com";

//        return new ExternalLoginUserInfo(email)
//        {
//            Name = GetAttribute(entry, "givenName") ?? userName,
//            Surname = GetAttribute(entry, "sn"),
//            EmailConfirmed = true,
//            ProviderKey = entry?.Dn ?? userName,
//        };
//    }

//    private static string? GetAttribute(LdapEntry? entry, string attributeName)
//    {
//        if (
//            entry != null
//            && entry.DirectoryAttributes.TryGetValue(attributeName, out var attribute)
//        )
//        {
//            return attribute.GetValue<string>();
//        }

//        return null;
//    }
//}
