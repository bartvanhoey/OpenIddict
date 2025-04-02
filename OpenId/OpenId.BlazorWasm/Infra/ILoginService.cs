using Microsoft.AspNetCore.Components;

namespace OpenId.BlazorWasm.Infra;

public interface ILoginService
{
    Task<LoginResult?> LoginUserAsync(string userName, string password);
}