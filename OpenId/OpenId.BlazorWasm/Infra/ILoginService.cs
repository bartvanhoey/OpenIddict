using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace OpenId.BlazorWasm.Infra;

public interface ILoginService
{
    Task<LoginResult?> LoginUserAsync(string userName, string password);
}

public class LoginService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authState) : ILoginService
{
    // Bugfix
    // ConstructorContainsNullParameterNames, System.Net.Http.FormUrlEncodedContent System.NotSupportedException: ConstructorContainsNullParameterNames, System.Net.Http.FormUrlEncodedContent
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Dictionary<string, string>))]
    private readonly Dictionary<string, string> values = new()
    {
        { "grant_type", "password" },
        // { "username", "bartvanhoey@hotmail.com" },
        // { "password", "Server2008!" },
        { "client_id", "blazorwasm-oidc-application" },
        { "client_secret", "388D45FA-B36B-4988-BA59-B187D329C206" },
        { "scope", "offline_access" }
    };
    

    public async Task<LoginResult?> LoginUserAsync(string userName, string password)
    {
        values.Add("username", userName);
        values.Add("password", password);
      
        var httpclient = httpClientFactory.CreateClient("AuthorityHttpClient");
        httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        var formUrlEncodedContent = new FormUrlEncodedContent(values);
        var response = await httpclient.PostAsync("connect/token", formUrlEncodedContent);
        if (response.StatusCode != HttpStatusCode.OK) return null;
        // var content = await response.Content.ReadAsStringAsync();
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        return result;
        await ((CustomAuthenticationStateProvider)authState).UpdateAuthenticationState(result?.access_token, result?.refresh_token);
        // NavigationManager.NavigateTo("/");
        return new LoginResult();
    }
}