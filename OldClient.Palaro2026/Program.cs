using Blazored.LocalStorage;
using OldClient.Palaro2026;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// Configure MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Add HttpClient with base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Blazored.LocalStorage for local storage
builder.Services.AddBlazoredLocalStorage();

// Register services
builder.Services.AddScoped<MapBoxService>();
builder.Services.AddScoped<APIService>();
builder.Services.AddScoped<ColorService>();

// Add authorization core for custom authentication
builder.Services.AddAuthorizationCore();

// Register the custom cookie service for managing cookies
builder.Services.AddScoped<CookieService>();

// Register the JWT authentication service
builder.Services.AddScoped<AuthenticationService>();

// Configure JWT bearer token handler for HttpClient
builder.Services.AddScoped(sp =>
{
    var client = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    // Do not hardcode the token; set it dynamically in your AuthenticationService after login
    return client;
});

// Optionally, if you need to configure an authorization header later, you can do so in the AuthenticationService
// where you manage the login process and set the token.

await builder.Build().RunAsync();