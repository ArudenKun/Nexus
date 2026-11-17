using Duende.IdentityModel.OidcClient.Browser;
using NativeWebView.Auth;
using NativeWebView.Core;

namespace Nexus;

public class AuthBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(
        BrowserOptions options,
        CancellationToken cancellationToken = default
    )
    {
        using var broker = new WebAuthenticationBroker();
        var authResult = await broker.AuthenticateAsync(
            new Uri(options.StartUrl),
            new Uri(options.EndUrl),
            WebAuthenticationOptions.None,
            cancellationToken
        );

        var result = new BrowserResult { ResultType = BrowserResultType.UserCancel };

        switch (authResult.ResponseStatus)
        {
            case WebAuthenticationStatus.Success:
                result.Response = authResult.ResponseData;
                result.ResultType = BrowserResultType.Success;
                break;
            case WebAuthenticationStatus.ErrorHttp:
                result.Error = authResult.ResponseData;
                result.ErrorDescription = authResult.ResponseErrorDetail.ToString();
                result.ResultType = BrowserResultType.HttpError;
                break;
            case WebAuthenticationStatus.UserCancel:
                break;
            default:
                throw new IndexOutOfRangeException("Unknown authentication type");
        }

        return result;
    }
}
