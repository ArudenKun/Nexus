using Nexus.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Nexus.Permissions;

public class NexusPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(NexusPermissions.GroupName);

        var booksPermission = myGroup.AddPermission(NexusPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(NexusPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(NexusPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(NexusPermissions.Books.Delete, L("Permission:Books.Delete"));

        var authorsPermission = myGroup.AddPermission(NexusPermissions.Authors.Default, L("Permission:Authors"));
        authorsPermission.AddChild(NexusPermissions.Authors.Create, L("Permission:Authors.Create"));
        authorsPermission.AddChild(NexusPermissions.Authors.Edit, L("Permission:Authors.Edit"));
        authorsPermission.AddChild(NexusPermissions.Authors.Delete, L("Permission:Authors.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(NexusPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<NexusResource>(name);
    }
}
