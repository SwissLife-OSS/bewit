using Azure.Core;
using Azure.Identity;
using MongoDB.Driver.Authentication.Oidc;

namespace Bewit.Storage.MongoDB;

internal sealed class BewitOidcCallback(List<string> scopes) : IOidcCallback
{
    private static readonly TimeSpan ExpirationBuffer = TimeSpan.FromMinutes(1);
    private readonly DefaultAzureCredential _credential = new();

    public OidcAccessToken GetOidcAccessToken(
        OidcCallbackParameters parameters,
        CancellationToken cancellationToken)
    {
        AccessToken accessToken = _credential.GetToken(
            new TokenRequestContext([.. scopes]), cancellationToken);

        return ToOidcAccessToken(accessToken);
    }

    public async Task<OidcAccessToken> GetOidcAccessTokenAsync(
        OidcCallbackParameters parameters,
        CancellationToken cancellationToken)
    {
        AccessToken accessToken = await _credential.GetTokenAsync(
            new TokenRequestContext([.. scopes]), cancellationToken);

        return ToOidcAccessToken(accessToken);
    }

    private static OidcAccessToken ToOidcAccessToken(AccessToken accessToken)
    {
        TimeSpan expiresIn = accessToken.ExpiresOn - DateTimeOffset.UtcNow - ExpirationBuffer;

        return new(accessToken.Token, expiresIn);
    }
}
