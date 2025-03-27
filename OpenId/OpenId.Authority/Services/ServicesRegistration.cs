namespace OpenId.Authority.Services;

public static class ServicesRegistration
{
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<AuthService>();
        builder.Services.AddTransient<ClientSeeder>();
        
    }
}