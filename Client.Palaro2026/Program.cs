using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client.Palaro2026;
using MudBlazor.Services;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

// Register services
builder.Services.AddScoped<MapBoxService>();
builder.Services.AddScoped<APIService>();
var host = builder.Build();
await host.RunAsync();
try
{
    using var scope = host.Services.CreateScope();
    var js = scope.ServiceProvider.GetRequiredService<IJSRuntime>();

    // Use development or production service worker based on environment
    var swFile = builder.HostEnvironment.IsDevelopment()
        ? "/service-worker.js"
        : "/service-worker.published.js";

    await js.InvokeVoidAsync("navigator.serviceWorker.register", swFile);
    Console.WriteLine($"Service Worker registered: {swFile}");
}
catch (Exception ex)
{
    Console.WriteLine($"Service Worker registration failed: {ex.Message}");
}