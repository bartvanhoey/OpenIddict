namespace OpenId.BlazorWasm.Infra;

public class LoginResult 
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ValidTo { get; set; }
}