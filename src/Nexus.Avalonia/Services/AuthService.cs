using System.Security.Claims;
using System.Text.Json;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;

namespace Nexus.Services;

public sealed record AuthResult(
    bool Succeeded,
    string? Error = null,
    string? ErrorDescription = null
);

public sealed record TokenRefreshedEventArgs(string AccessToken, DateTimeOffset Expiration);

public class AuthService : IDisposable, ISingletonDependency
{
    private static readonly TimeSpan ExpirationBuffer = TimeSpan.FromSeconds(60);
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _accessToken;
    private string? _refreshToken;
    private string? _identityToken;
    private DateTimeOffset _accessTokenExpiration = DateTimeOffset.MinValue;

    public required ITransientCachedServiceProvider LazyServiceProvider { get; init; }

    protected AuthOptions AuthOptions =>
        LazyServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;

    protected ILoggerFactory LoggerFactory =>
        LazyServiceProvider.GetRequiredService<ILoggerFactory>();

    protected ILogger Logger =>
        LazyServiceProvider.GetService<ILogger>(_ =>
            LoggerFactory.CreateLogger(GetType().FullName!)
        );

    protected ILocalEventBus LocalEventBus =>
        LazyServiceProvider.GetRequiredService<ILocalEventBus>();

    protected IHttpClientFactory HttpClientFactory =>
        LazyServiceProvider.GetRequiredService<IHttpClientFactory>();

    protected OidcClient OidcClient =>
        field ??= new OidcClient(
            new OidcClientOptions
            {
                Authority = AuthOptions.Authority,
                ClientId = AuthOptions.ClientId,
                Scope = AuthOptions.Scopes,
                RedirectUri = AuthOptions.RedirectUrl,
                PostLogoutRedirectUri = AuthOptions.PostLogoutRedirectUrl,
                DisablePushedAuthorization = !AuthOptions.PushedAuthorization,
                Browser = new AuthBrowser(),
            }
        );

    public bool IsAuthenticated =>
        !string.IsNullOrEmpty(_accessToken)
        && (DateTimeOffset.UtcNow < _accessTokenExpiration || !string.IsNullOrEmpty(_refreshToken));

    public ClaimsPrincipal CurrentUser { get; private set; } = new(new ClaimsIdentity());

    public string? UserId =>
        CurrentUser.FindFirst("sub")?.Value
        ?? CurrentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? UserName =>
        CurrentUser.FindFirst("preferred_username")?.Value
        ?? CurrentUser.FindFirst("name")?.Value
        ?? CurrentUser.Identity?.Name;

    public string? Email =>
        CurrentUser.FindFirst("email")?.Value ?? CurrentUser.FindFirst(ClaimTypes.Email)?.Value;

    public Guid? TenantId =>
        Guid.TryParse(CurrentUser.FindFirst("tenantid")?.Value, out var id) ? id : null;

    public IReadOnlyList<string> Roles =>
        [
            .. CurrentUser
                .FindAll("role")
                .Concat(CurrentUser.FindAll(ClaimTypes.Role))
                .Select(c => c.Value)
                .Distinct(),
        ];

    public event EventHandler<bool>? AuthenticationStateChanged;
    public event EventHandler<TokenRefreshedEventArgs>? TokenRefreshed;
    public event EventHandler? TokenExpired;
    public event EventHandler? Logout;

    /// <summary>
    /// Logs in interactively using Authorization Code + PKCE via the configured <see cref="AuthBrowser"/>.
    /// </summary>
    public async Task<AuthResult> LoginAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await OidcClient.LoginAsync(new LoginRequest(), cancellationToken);

            if (result.IsError)
            {
                return new AuthResult(false, result.Error, result.ErrorDescription);
            }

            await SetAuthenticatedStateAsync(
                accessToken: result.AccessToken,
                refreshToken: result.RefreshToken,
                identityToken: result.IdentityToken,
                expiration: result.AccessTokenExpiration,
                user: result.User
            );

            return new AuthResult(true);
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "An error occurred while logging in with browser");
            return new AuthResult(
                false,
                "Login Failed",
                "An error occurred while logging in with the browser"
            );
        }
    }

    /// <summary>
    /// Logs in directly using a username and password (Resource Owner Password Credentials grant).
    /// </summary>
    public async Task<AuthResult> LoginAsync(
        string userName,
        string password,
        string? tenant = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
        {
            return new AuthResult(false, "invalid_request", "Username and password are required.");
        }

        try
        {
            var httpClient = HttpClientFactory.CreateClient();
            var disco = await httpClient.GetDiscoveryDocumentAsync(
                AuthOptions.Authority,
                cancellationToken
            );
            if (disco.IsError)
            {
                return new AuthResult(false, disco.Error ?? "discovery_error", disco.Error);
            }

            var tokenRequest = new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = AuthOptions.ClientId,
                UserName = userName,
                Password = password,
                Scope = AuthOptions.Scopes,
            };

            if (!string.IsNullOrWhiteSpace(tenant))
            {
                tokenRequest.Headers.Add("__tenant", tenant);
            }

            var tokenResponse = await httpClient.RequestPasswordTokenAsync(
                tokenRequest,
                cancellationToken
            );

            if (tokenResponse.IsError || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return new AuthResult(
                    false,
                    tokenResponse.Error ?? "invalid_grant",
                    tokenResponse.ErrorDescription
                );
            }

            var user = await BuildClaimsPrincipalAsync(
                httpClient,
                disco,
                tokenResponse.AccessToken,
                cancellationToken
            );

            var expiration =
                tokenResponse.ExpiresIn > 0
                    ? DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                    : DateTimeOffset.MaxValue;

            await SetAuthenticatedStateAsync(
                accessToken: tokenResponse.AccessToken,
                refreshToken: tokenResponse.RefreshToken,
                identityToken: tokenResponse.IdentityToken,
                expiration: expiration,
                user: user
            );

            return new AuthResult(true);
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "An error occurred while logging in with username and password");
            return new AuthResult(
                false,
                "Login Failed",
                "An error occurred while logging in with username and password"
            );
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var idToken = _identityToken;
        var refreshToken = _refreshToken;

        await ClearStateAsync();

        try
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var httpClient = HttpClientFactory.CreateClient();
                var disco = await httpClient.GetDiscoveryDocumentAsync(
                    AuthOptions.Authority,
                    cancellationToken
                );

                if (!disco.IsError && !string.IsNullOrEmpty(disco.RevocationEndpoint))
                {
                    await httpClient.RevokeTokenAsync(
                        new TokenRevocationRequest
                        {
                            Address = disco.RevocationEndpoint,
                            ClientId = AuthOptions.ClientId,
                            Token = refreshToken,
                            TokenTypeHint = "refresh_token",
                        },
                        cancellationToken
                    );
                }
            }

            // Only trigger front-channel browser logout if an interactive browser session existed.
            if (!string.IsNullOrEmpty(idToken))
            {
                await OidcClient.LogoutAsync(
                    new LogoutRequest { IdTokenHint = idToken },
                    cancellationToken
                );
            }
        }
        catch
        {
            // Local session is already cleared; ignore browser close/network errors on logout.
        }
        finally
        {
            Logout?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Returns an active access token, silently refreshing it if expired or within the safety buffer.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (
            !string.IsNullOrEmpty(_accessToken)
            && DateTimeOffset.UtcNow.Add(ExpirationBuffer) < _accessTokenExpiration
        )
        {
            return _accessToken;
        }

        if (string.IsNullOrEmpty(_refreshToken))
        {
            if (string.IsNullOrEmpty(_accessToken))
                return null;
            await ClearStateAsync();
            TokenExpired?.Invoke(this, EventArgs.Empty);

            return null;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock in case another concurrent request already refreshed the token.
            if (
                !string.IsNullOrEmpty(_accessToken)
                && DateTimeOffset.UtcNow.Add(ExpirationBuffer) < _accessTokenExpiration
            )
            {
                return _accessToken;
            }

            if (string.IsNullOrEmpty(_refreshToken))
            {
                if (string.IsNullOrEmpty(_accessToken))
                    return null;
                await ClearStateAsync();
                TokenExpired?.Invoke(this, EventArgs.Empty);

                return null;
            }

            var refreshResult = await OidcClient.RefreshTokenAsync(
                _refreshToken,
                cancellationToken: cancellationToken
            );

            if (refreshResult.IsError)
            {
                Logger.LogError("An error occurred while refreshing token");
                await ClearStateAsync();
                TokenExpired?.Invoke(this, EventArgs.Empty);
                return null;
            }

            // OpenIddict rotates refresh tokens by default, while id_token may be omitted on refresh.
            var newRefreshToken = !string.IsNullOrEmpty(refreshResult.RefreshToken)
                ? refreshResult.RefreshToken
                : _refreshToken;

            var newIdentityToken = !string.IsNullOrEmpty(refreshResult.IdentityToken)
                ? refreshResult.IdentityToken
                : _identityToken;

            await SetAuthenticatedStateAsync(
                accessToken: refreshResult.AccessToken,
                refreshToken: newRefreshToken,
                identityToken: newIdentityToken,
                expiration: refreshResult.AccessTokenExpiration,
                user: CurrentUser
            );

            TokenRefreshed?.Invoke(
                this,
                new TokenRefreshedEventArgs(
                    refreshResult.AccessToken,
                    refreshResult.AccessTokenExpiration
                )
            );

            return _accessToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void ManualExpire()
    {
        TokenExpired?.Invoke(this, EventArgs.Empty);
    }

    private static async Task<ClaimsPrincipal> BuildClaimsPrincipalAsync(
        HttpClient httpClient,
        DiscoveryDocumentResponse disco,
        string accessToken,
        CancellationToken cancellationToken
    )
    {
        var claims = new List<Claim>(ParseJwtClaims(accessToken));

        if (!string.IsNullOrEmpty(disco.UserInfoEndpoint))
        {
            var userInfo = await httpClient.GetUserInfoAsync(
                new UserInfoRequest { Address = disco.UserInfoEndpoint, Token = accessToken },
                cancellationToken
            );

            if (!userInfo.IsError)
            {
                foreach (var claim in userInfo.Claims)
                {
                    if (!claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                    {
                        claims.Add(claim);
                    }
                }
            }
        }

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Password",
            nameType: "preferred_username",
            roleType: "role"
        );

        return new ClaimsPrincipal(identity);
    }

    private static IEnumerable<Claim> ParseJwtClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
        {
            yield break;
        }

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload += (payload.Length % 4) switch
        {
            2 => "==",
            3 => "=",
            _ => string.Empty,
        };

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(payload);
        }
        catch
        {
            yield break;
        }

        using var doc = JsonDocument.Parse(bytes);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in prop.Value.EnumerateArray())
                {
                    yield return new Claim(prop.Name, item.ToString());
                }
            }
            else if (prop.Value.ValueKind != JsonValueKind.Null)
            {
                yield return new Claim(prop.Name, prop.Value.ToString());
            }
        }
    }

    private Task SetAuthenticatedStateAsync(
        string accessToken,
        string? refreshToken,
        string? identityToken,
        DateTimeOffset expiration,
        ClaimsPrincipal user
    )
    {
        var wasAuthenticated = IsAuthenticated;

        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _identityToken = identityToken;
        _accessTokenExpiration = expiration;

        if (user.Identity?.IsAuthenticated == true)
        {
            CurrentUser = user;
        }

        if (!wasAuthenticated || IsAuthenticated)
        {
            AuthenticationStateChanged?.Invoke(this, true);
        }

        return Task.CompletedTask;
    }

    private Task ClearStateAsync()
    {
        var wasAuthenticated = IsAuthenticated;

        _accessToken = null;
        _refreshToken = null;
        _identityToken = null;
        _accessTokenExpiration = DateTimeOffset.MinValue;
        CurrentUser = new ClaimsPrincipal(new ClaimsIdentity());

        if (wasAuthenticated)
        {
            AuthenticationStateChanged?.Invoke(this, false);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _refreshLock.Dispose();
    }

    public static class Events
    {
        private const string Group = "AuthService";
        public const string AuthenticationStateChanged = Group + ".AuthenticationStateChanged";
        public const string TokenRefreshed = Group + ".TokenRefreshed";
        public const string TokenExpired = Group + ".TokenExpired";
    }
}
