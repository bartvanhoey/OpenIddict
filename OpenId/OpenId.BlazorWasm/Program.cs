using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpenId.BlazorWasm;
using OpenId.BlazorWasm.Infra;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddAuthorizationCore();

var serverBaseAddress = builder.Configuration["AuthorityUrl"] ?? "http://localhost:7000";

builder.Services.AddTransient<CustomAuthenticationHandler>();
builder.Services.AddHttpClient("ApiHttpClient",
        client =>
        {
            client.BaseAddress = new Uri(serverBaseAddress);
        })
    .AddHttpMessageHandler<CustomAuthenticationHandler>();

builder.Services.RegisterServices();

await builder.Build().RunAsync();