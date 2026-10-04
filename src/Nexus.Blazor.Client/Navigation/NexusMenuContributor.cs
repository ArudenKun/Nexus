using System;
using System.Threading.Tasks;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.Configuration;
using Nexus.Localization;
using Nexus.MultiTenancy;
using Nexus.Permissions;
using Volo.Abp.Account.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Blazor.MudBlazor;
using Volo.Abp.SettingManagement.Blazor.MudBlazor.Menus;
using Volo.Abp.TenantManagement.Blazor.MudBlazor.Navigation;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Users;

namespace Nexus.Blazor.Client.Navigation;

public class NexusMenuContributor : IMenuContributor
{
    private readonly IConfiguration _configuration;

    public NexusMenuContributor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
        }
    }

    private static async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<NexusResource>();

        //Administration
        var administration = context.Menu.GetAdministration();
        administration.Order = 6;
        context.Menu.AddItem(
            new ApplicationMenuItem(
                NexusMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "bi bi-house",
                order: 1
            )
        );

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);

        var bookStoreMenu = new ApplicationMenuItem(
            "BooksStore",
            l["Menu:Nexus"],
            icon: "fa fa-book"
        );
        context.Menu.AddItem(bookStoreMenu);
        if (await context.IsGrantedAsync(NexusPermissions.Books.Default))
        {
            bookStoreMenu.AddItem(
                new ApplicationMenuItem("BooksStore.Books", l["Menu:Books"], url: "/books")
            );
        }
        if (await context.IsGrantedAsync(NexusPermissions.Authors.Default))
        {
            bookStoreMenu.AddItem(
                new ApplicationMenuItem("BooksStore.Authors", l["Menu:Authors"], url: "/authors")
            );
        }
    }

    private async Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        if (OperatingSystem.IsBrowser())
        {
            //Blazor wasm menu items
            var authServerUrl = _configuration["AuthServer:Authority"] ?? "";
            var accountResource = context.GetLocalizer<AccountResource>();
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "Account.Manage",
                    accountResource["MyAccount"],
                    $"{authServerUrl.EnsureEndsWith('/')}Account/Manage",
                    icon: "fa fa-cog",
                    order: 900,
                    target: "_blank"
                ).RequireAuthenticated()
            );
        }
        else
        {
            //Blazor server menu items
        }
        await Task.CompletedTask;
    }
}
