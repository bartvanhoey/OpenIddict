using System.Net.Http.Json;
using OpenId.Shared.Models;
using static System.String;
using static OpenId.BlazorWasm.Infra.AuthRefreshMessage;

namespace OpenId.BlazorWasm.Infra;

public class RefreshService(
    IHttpClientFactory clientFactory,
    IJwtTokenService jwtTokenService,
    ILogoutService? logoutService = null)
{
    public async Task<AuthRefreshResult> RefreshAsync(string? accessToken, string? refreshToken)
    {
        if (IsNullOrWhiteSpace(accessToken) ) { return new AuthRefreshResult(InputAccessTokenNull ); }
        if (IsNullOrWhiteSpace(refreshToken)) { return new AuthRefreshResult(InputRefreshTokenNull); }

        var httpClient = clientFactory.CreateClient("AuthorityHttpClient");
        
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/account/refresh", new RefreshInputModel(accessToken, refreshToken));
            if (response is { IsSuccessStatusCode: true })
            {
                var result = await response.Content.ReadFromJsonAsync<RefreshResult>();
                
                if (result == null) return new AuthRefreshResult(ResponseContentNull);
                if (IsNullOrWhiteSpace(result.access_token)) return new AuthRefreshResult(AccessTokenNull);
                if (IsNullOrWhiteSpace(result.refresh_token)) return new AuthRefreshResult(RefreshTokenNull);
                if (!jwtTokenService.IsValid(result.access_token)) return new AuthRefreshResult(AccessTokenInvalid);

                await jwtTokenService.SaveAuthTokensAsync(result.access_token, result.refresh_token);

                return new AuthRefreshResult(result.access_token, result.refresh_token, result.expires_in);
            }
        }
        catch (Exception)
        {
            if (logoutService != null) await logoutService.LogoutAsync();
            return new AuthRefreshResult(ExceptionThrown);
        }

        if (logoutService != null) await logoutService.LogoutAsync();
        return new AuthRefreshResult(SomethingWentWrong);
    }
}