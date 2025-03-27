namespace OpenId.BlazorWasm.Infra;

public static class ServicesRegistration
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ILogoutService, LogoutService>();
    }
}