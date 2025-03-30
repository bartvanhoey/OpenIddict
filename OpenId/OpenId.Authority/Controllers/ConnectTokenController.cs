using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenId.Authority.Data;
using OpenId.Authority.Extensions;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OpenId.Authority.Controllers;

public class ConnectTokenController(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    : BaseController
{
    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null) return BadRequest("The request is missing.");
        if (request.IsClientCredentialsGrantType()) return await ProcessClientCredentialsGrantType(request);
        if (request.IsPasswordGrantType()) return await ProcessPasswordGrantType(request);
        if (request.IsRefreshTokenGrantType()) return await ProcessRefreshTokenGrantType();
        throw new NotImplementedException("The specified grant is not implemented.");
    }

    private async Task<IActionResult> ProcessClientCredentialsGrantType(OpenIddictRequest request)
    {
        // Note: the client credentials are automatically validated by OpenIddict:
        // if client_id or client_secret are invalid, this action won't be invoked.
        var application =
            await applicationManager.FindByClientIdAsync(request.ClientId ?? throw new InvalidOperationException()) ??
            throw new InvalidOperationException("The application cannot be found.");
        // Create a new ClaimsIdentity containing the claims that will be used to create an id_token, a token or a code.
        var identity =
            new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

        // Use the client_id as the subject identifier.
        identity.SetClaim(Claims.Subject, await applicationManager.GetClientIdAsync(application));
        identity.SetClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application));
        identity.AddClaim(new Claim(Claims.Audience, "Resourse"));
        identity.AddClaim(new Claim("some-claim", "some-value"));
        identity.SetDestinations(static claim => claim.Type switch
        {
            // Allow the "name" claim to be stored in both the access and identity tokens
            // when the "profile" scope was granted (by calling principal.SetScopes(...)).
            Claims.Name when (claim.Subject ?? throw new InvalidOperationException()).HasScope(Permissions.Scopes
                    .Profile)
                => [Destinations.AccessToken, Destinations.IdentityToken],

            // Otherwise, only store the claim in the access tokens.
            _ => [Destinations.AccessToken]
        });
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ProcessRefreshTokenGrantType()
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Retrieve the user profile corresponding to the refresh token.
        var user = await userManager.FindByIdAsync(result.Principal?.GetClaim(Claims.Subject) ?? string.Empty);
        if (user == null) return Forbidden("The refresh token is no longer valid.");
        
        // Ensure the user is still allowed to sign in.
        if (!await signInManager.CanSignInAsync(user)) return Forbidden("The user is no longer allowed to sign in.");
        
        var identity = new ClaimsIdentity(result.Principal?.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Override the user claims present in the principal in case they changed since the refresh token was issued.
        identity.SetClaim(Claims.Subject, await userManager.GetUserIdAsync(user))
            .SetClaim(Claims.Email, await userManager.GetEmailAsync(user))
            .SetClaim(Claims.Name, await userManager.GetUserNameAsync(user))
            .SetClaim(Claims.PreferredUsername, await userManager.GetUserNameAsync(user))
            .SetClaims(Claims.Role, [.. await userManager.GetRolesAsync(user)]);

        identity.SetDestinations(ClaimExtensions.GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ProcessPasswordGrantType(OpenIddictRequest request)
    {
        // if client_id or client_secret are invalid, this action won't be invoked.
        try
        {
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, Claims.Name,
                Claims.Role);
            AuthenticationProperties properties = new();

            var user = await userManager.FindByNameAsync(request.Username ?? throw new InvalidOperationException());
            if (user == null) return Nok500(Errors.InvalidGrant, "User does not exist");

            // Check that the user can sign in and is not locked out.
            // If two-factor authentication is supported, it would also be appropriate to check that 2FA is enabled for the user
            if (!await signInManager.CanSignInAsync(user) ||
                (userManager.SupportsUserLockout && await userManager.IsLockedOutAsync(user)))
                return Nok500(Errors.InvalidGrant, "The specified user cannot sign in");

            // Validate the username/password parameters and ensure the account is not locked out.
            var result = await signInManager.PasswordSignInAsync(user.UserName ?? string.Empty, request.Password ?? string.Empty,
                false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                if (result.IsNotAllowed)
                    return Nok500(Errors.InvalidGrant, "User not allowed to login. Please confirm your email");
                if (result.RequiresTwoFactor) return Nok500(Errors.InvalidGrant, "User requires 2F authentication");
                if (result.IsLockedOut) return Nok500(Errors.InvalidGrant, "User is locked out");
                return Nok500(Errors.InvalidGrant, "Username or password is incorrect");
            }

            // The user is now validated, so reset lockout counts, if necessary
            if (userManager.SupportsUserLockout) await userManager.ResetAccessFailedCountAsync(user);

            //// Getting scopes from user parameters (TokenViewModel) and adding in Identity 
            identity.SetScopes(request.GetScopes());

            // Getting scopes from user parameters (TokenViewModel)
            // Checking in OpenIddictScopes tables for matching resources
            // Adding in Identity
            identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

            // Add Custom claims => sub claims are mandatory
            identity.AddClaim(new Claim(Claims.Subject, user.Id));
            identity.AddClaim(new Claim(Claims.PreferredUsername, (user.Email ?? user.UserName) ?? throw new InvalidOperationException()));
            identity.AddClaim(new Claim(Claims.Audience, "Resourse"));
            identity.AddClaim(new Claim("some-claim", "some-value"));

            // Setting destinations of claims i.e., identity token or access token

            // When using this statement, custom claims aren't included in AccessToken
            // identity.SetDestinations(x => GetDestinations(x, identity));

            identity.SetDestinations(static claim => claim.Type switch
            {
                // Allow the "name" claim to be stored in both the access and identity tokens
                // when the "profile" scope was granted (by calling principal.SetScopes(...)).
                Claims.Name when (claim.Subject ?? throw new InvalidOperationException()).HasScope(
                        Permissions.Scopes.Profile)
                    => [Destinations.AccessToken, Destinations.IdentityToken],

                // Otherwise, only store the claim in the access tokens.
                _ => [Destinations.AccessToken]
            });

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            var signInResult = SignIn(new ClaimsPrincipal(identity), properties,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return signInResult;
        }
        catch (Exception)
        {
            return Nok500(Errors.ServerError, "Invalid login attempt");
        }
    }
}