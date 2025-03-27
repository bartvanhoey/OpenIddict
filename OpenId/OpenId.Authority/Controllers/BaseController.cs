using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace OpenId.Authority.Controllers;

public class BaseController : Controller
{
    protected IActionResult Nok500(string error, string description) => BadRequest(new OpenIddictResponse
        {
            Error =  error,
            ErrorDescription = description
        });


    protected void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
    }

}