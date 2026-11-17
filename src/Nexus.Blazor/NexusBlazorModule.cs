using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using global::MudBlazor;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Nexus.Blazor.Client;
using Nexus.Blazor.Client.Navigation;
using Nexus.Blazor.Components;
using Nexus.EntityFrameworkCore;
using Nexus.Localization;
using Nexus.MultiTenancy;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Components.Server.MudBlazorBasicTheme;
using Volo.Abp.AspNetCore.Components.Server.MudBlazorBasicTheme.Bundling;
using Volo.Abp.AspNetCore.Components.Web;
using Volo.Abp.AspNetCore.Components.Web.Theming.MudBlazor.Routing;
using Volo.Abp.AspNetCore.Components.WebAssembly.MudBlazorBasicTheme.Bundling;
using Volo.Abp.AspNetCore.Components.WebAssembly.Theming.MudBlazor.Bundling;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Blazor.MudBlazor.Server;
using Volo.Abp.Localization;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.MudBlazorUI;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace Nexus.Blazor;

[DependsOn(
    typeof(NexusApplicationModule),
    typeof(NexusEntityFrameworkCoreModule),
    typeof(NexusHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpIdentityBlazorMudBlazorServerModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreComponentsServerMudBlazorBasicThemeModule),
    typeof(AbpAspNetCoreComponentsWebAssemblyMudBlazorBasicThemeBundlingModule),
    typeof(AbpAspNetCoreMvcUiBasicThemeModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpCachingStackExchangeRedisModule)
)]
public class NexusBlazorModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(NexusResource),
                typeof(NexusDomainModule).Assembly,
                typeof(NexusDomainSharedModule).Assembly,
                typeof(NexusApplicationModule).Assembly,
                typeof(NexusApplicationContractsModule).Assembly,
                typeof(NexusBlazorModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("Nexus");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate(
                    "openiddict.pfx",
                    configuration["AuthServer:CertificatePassPhrase"]!
                );
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }

        PreConfigure<AbpAspNetCoreComponentsWebOptions>(options =>
        {
            options.IsBlazorWebApp = true;
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        // Add services to the container.
        context
            .Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact =
                true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });
        }

        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles(hostingEnvironment);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureSwaggerServices(context.Services);
        ConfigureAutoApiControllers();
        ConfigureBlazorise(context);
        ConfigureRouter(context);
        ConfigureMenu(context);
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(
            OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
        );
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(
                configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>()
            );
        });
    }

    private void ConfigureBundles(IHostEnvironment hostingEnvironment)
    {
        Configure<AbpBundlingOptions>(options =>
        {
            // Blazor Web App
            options.Parameters.InteractiveAuto = true;

            // MVC UI
            options.StyleBundles.Configure(BasicThemeBundles.Styles.Global, bundle =>
            {
                bundle.AddFiles("/global-styles.css");
            });

            options.ScriptBundles.Configure(BasicThemeBundles.Scripts.Global, bundle =>
            {
                bundle.AddFiles("/global-scripts.js");

                if (hostingEnvironment.IsDevelopment())
                {
                    bundle.AddFiles("/dev-login-helper.js");
                }
            });

            // Blazor UI
            options.StyleBundles.Configure(BlazorMudBlazorBasicThemeBundles.Styles.Global, bundle =>
            {
                bundle.AddFiles("/global-styles.css");
            });
        });

        Configure<AbpBundlingOptions>(options =>
        {
            var globalStyles = options.StyleBundles.Get(
                BlazorWebAssemblyMudBlazorStandardBundles.Styles.Global
            );
            globalStyles.AddContributors(typeof(NexusStyleBundleContributor));

            var globalScripts = options.ScriptBundles.Get(
                BlazorWebAssemblyMudBlazorStandardBundles.Scripts.Global
            );
            globalScripts.AddContributors(typeof(NexusScriptBundleContributor));
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<NexusDomainSharedModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Nexus.Domain.Shared"
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<NexusDomainModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Nexus.Domain"
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<NexusApplicationContractsModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Nexus.Application.Contracts"
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<NexusApplicationModule>(
                    Path.Combine(
                        hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Nexus.Application"
                    )
                );
                options.FileSets.ReplaceEmbeddedByPhysical<NexusBlazorModule>(
                    hostingEnvironment.ContentRootPath
                );
            });
        }
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Nexus API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.CustomSchemaIds(type => type.FullName);
        });
    }

    private void ConfigureBlazorise(ServiceConfigurationContext context)
    {
        // MudBlazor services are registered by AbpMudBlazorUIModule.
    }

    private void ConfigureMenu(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(
                new NexusMenuContributor(context.Services.GetConfiguration())
            );
        });
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<AbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(NexusBlazorModule).Assembly;
            options.AdditionalAssemblies.Add(typeof(NexusBlazorClientModule).Assembly);
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(NexusApplicationModule).Assembly);
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var env = context.GetEnvironment();
        var app = context.GetApplicationBuilder();

        app.Use(
            async (ctx, next) =>
            {
                /* Converting to https to be able to include https URLs in `/.well-known/openid-configuration` endpoint.
                 * This should only be done if the request is coming outside of the cluster.  */
                if (ctx.Request.Headers.ContainsKey("from-ingress"))
                {
                    ctx.Request.Scheme = "https";
                }

                await next();
            }
        );

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
            app.UseHsts();
        }

        app.UseCorrelationId();
        app.UseRouting();
        var configuration = context.GetConfiguration();
        if (Convert.ToBoolean(configuration["AuthServer:IsOnK8s"]))
        {
            app.Use(
                async (context, next) =>
                {
                    if (
                        context.Request.Path.Value != null
                        && context.Request.Path.Value.StartsWith(
                            "/appsettings",
                            StringComparison.OrdinalIgnoreCase
                        )
                        && context.Request.Path.Value.EndsWith(
                            ".json",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        // Set endpoint to null so the static files middleware will handle the request.
                        context.SetEndpoint(null);
                    }
                    await next(context);
                }
            );

            app.UseStaticFilesForPatterns("appsettings*.json");
        }
        app.MapAbpStaticAssets();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseAbpExceptionHandling();
        app.UseDynamicClaims();
        app.UseAntiforgery();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexus API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();

        app.UseConfiguredEndpoints(builder =>
        {
            builder
                .MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(
                    builder
                        .ServiceProvider.GetRequiredService<IOptions<AbpRouterOptions>>()
                        .Value.AdditionalAssemblies.ToArray()
                );
        });
    }
}
