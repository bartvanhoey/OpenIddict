using System.ComponentModel.DataAnnotations;

namespace OpenId.Shared.Models;

public class RefreshInputModel 
{
    public RefreshInputModel()
    {
    }

    public RefreshInputModel(string? accessToken, string? refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    [Required] public string? AccessToken { get; set; }
    [Required] public string? RefreshToken { get; set; }
}