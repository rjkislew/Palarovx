using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client.Palaro2026;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var environment = builder.HostEnvironment.Environment;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<MapBoxService>();
builder.Services.AddScoped<APIService>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();

