using Admin.Palaro2026;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

// Load configuration from appsettings.json
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Register APIService
builder.Services.AddSingleton<APIService>();

await builder.Build().RunAsync();
