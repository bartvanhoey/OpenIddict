using Microsoft.AspNetCore.Identity;
using OpenId.Authority.Data;

namespace OpenId.Authority.ServiceRegistration;

public static class IdentityRegistration
{
    public static void SetupIdentity(this WebApplicationBuilder builder) =>
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
            // .AddSignInManager()
            
            .AddDefaultUI()
            .AddDefaultTokenProviders();
}