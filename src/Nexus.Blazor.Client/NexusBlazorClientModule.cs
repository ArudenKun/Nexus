using System;
using System.Net.Http;
using global::MudBlazor;
using Volo.Abp.MudBlazorUI;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nexus.Blazor.Client.Navigation;
using OpenIddict.Abstractions;
using Volo.Abp.AspNetCore.Components.Web;
using Volo.Abp.AspNetCore.Components.Web.Theming.MudBlazor.Routing;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Components.WebAssembly.Theming.MudBlazor.Bundling;
using Volo.Abp.AspNetCore.Components.WebAssembly.MudBlazorBasicTheme;
using Volo.Abp.SettingManagement.Blazor.MudBlazor.WebAssembly;
using Volo.Abp.FeatureManagement.Blazor.MudBlazor.WebAssembly;
using Volo.Abp.Identity.Blazor.MudBlazor;
using Volo.Abp.Identity.Blazor.MudBlazor.WebAssembly;


namespace Nexus.Blazor.Client;

[DependsOn(
    typeof(AbpSettingManagementBlazorMudBlazorWebAssemblyModule),
    typeof(AbpFeatureManagementBlazorMudBlazorWebAssemblyModule),
    typeof(AbpIdentityBlazorMudBlazorWebAssemblyModule),
    typeof(AbpAspNetCoreComponentsWebAssemblyMudBlazorBasicThemeModule),
    typeof(AbpAutofacWebAssemblyModule),
    typeof(NexusHttpApiClientModule)
)]
public class NexusBlazorClientModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAspNetCoreComponentsWebOptions>(options =>
        {
            options.IsBlazorWebApp = true;
        });
    }
    
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();

        ConfigureAuthentication(builder);
        ConfigureHttpClient(context, environment);
        ConfigureBlazorise(context);
        ConfigureRouter(context);
        ConfigureMenu(context);
    }


    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<AbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(NexusBlazorClientModule).Assembly;
            options.AdditionalAssemblies.Add(typeof(NexusBlazorClientModule).Assembly);
        });
    }

    private void ConfigureMenu(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new NexusMenuContributor(context.Services.GetConfiguration()));
        });
    }

    private void ConfigureBlazorise(ServiceConfigurationContext context)
    {
        // MudBlazor services are registered by AbpMudBlazorUIModule.
    }

    private static void ConfigureAuthentication(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddBlazorWebAppServices();
    }
    

    private static void ConfigureHttpClient(ServiceConfigurationContext context, IWebAssemblyHostEnvironment environment)
    {
        context.Services.AddTransient(sp => new HttpClient
        {
            BaseAddress = new Uri(environment.BaseAddress)
        });
    }
}
