using Microsoft.EntityFrameworkCore;
using OpenId.Authority.Data;
using OpenId.Authority.ServiceRegistration;
using OpenId.Authority.Services;

namespace OpenId.Authority
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddControllers();

            builder.SetupDatabase();

            builder.SetupIdentity();

            builder.SetupOpenIddict();

            builder.AddCorsPolicy();

            builder.Services.AddTransient<AuthService>();
            builder.Services.AddTransient<ClientSeeder>();

            builder.Services.AddRazorPages();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await dbContext.Database.MigrateAsync();
                
                var clientSeeder = scope.ServiceProvider.GetRequiredService<ClientSeeder>();
                clientSeeder.AddClientCredentialsClient().GetAwaiter().GetResult();
                clientSeeder.AddPasswordClient().GetAwaiter().GetResult();
                clientSeeder.AddAuthorizationClient().GetAwaiter().GetResult();
                clientSeeder.AddWebClient().GetAwaiter().GetResult();
                clientSeeder.AddReactClient().GetAwaiter().GetResult();
                clientSeeder.AddBlazorWasmClient().GetAwaiter().GetResult();
                clientSeeder.AddScopes().GetAwaiter().GetResult();
            }
            
            
            if (app.Environment.IsDevelopment()) { app.UseMigrationsEndPoint(); }
            else
            {
                app.UseExceptionHandler("/Error");
                
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();

            app.MapStaticAssets();
            app.MapRazorPages()
                .WithStaticAssets();
            
            app.UseCors("CorsPolicy");

            app.Run();
        }
    }
}