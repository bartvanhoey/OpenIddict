using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenId.Authority.Data;
using OpenId.Authority.Model;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace OpenId.Authority.Controllers;

[Route("api/register")]
[ApiController]
public class RegisterController(UserManager<ApplicationUser> userManager) : BaseController
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterInputModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
               
        var user = await userManager.FindByNameAsync(model.Email);
        if (user != null) return StatusCode(Status409Conflict);

        user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await userManager.CreateAsync(user, model.Password);
        
        if (result.Succeeded) return Ok();
        
        AddErrors(result);

        return BadRequest(ModelState);
    }
}