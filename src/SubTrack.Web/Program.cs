using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SubTrack.Web;
using SubTrack.Web.ApiClient;
using SubTrack.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is not configured in wwwroot/appsettings.json.");

builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services
    .AddHttpClient<ISubTrackApiClient, SubTrackApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped<IAuthApi, AuthApi>();
builder.Services.AddScoped<ISubscriptionsApi, SubscriptionsApi>();
builder.Services.AddScoped<SubscriptionStateService>();

var host = builder.Build();

await host.Services.GetRequiredService<TokenStore>().InitializeAsync();

await host.RunAsync();
