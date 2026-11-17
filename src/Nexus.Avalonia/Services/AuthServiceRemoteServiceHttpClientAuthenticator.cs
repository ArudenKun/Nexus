using Duende.IdentityModel.Client;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client.Authentication;

namespace Nexus.Services;

[Dependency(ReplaceServices = true)]
public class AuthServiceRemoteServiceHttpClientAuthenticator
    : IRemoteServiceHttpClientAuthenticator,
        ITransientDependency
{
    private readonly AuthService _authService;

    public AuthServiceRemoteServiceHttpClientAuthenticator(AuthService authService)
    {
        _authService = authService;
    }

    public async Task Authenticate(RemoteServiceHttpClientAuthenticateContext context)
    {
        var accessToken = await _authService.GetAccessTokenAsync();
        if (!accessToken.IsNullOrEmpty())
        {
            context.Request.SetBearerToken(accessToken);
        }
    }
}
