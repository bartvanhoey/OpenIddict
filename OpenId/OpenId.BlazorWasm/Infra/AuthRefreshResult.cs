using static OpenId.BlazorWasm.Infra.AuthRefreshMessage;

namespace OpenId.BlazorWasm.Infra;

public class AuthRefreshResult(AuthRefreshMessage resultInfo, string? message = null) 
{
    public string? AccessToken { get; }
    public string? RefreshToken { get; }
    public DateTime ValidTo { get; set; }
    public bool Success => ResultInfo == Successful;
    public bool Failure => !Success;
    
    public string? Message { get; } = message;
    
    public AuthRefreshResult(string accessToken, string refreshToken, DateTime validTo) : this(Successful)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ValidTo = validTo;
    }

    private AuthRefreshMessage ResultInfo { get; } = resultInfo;
    
    
}