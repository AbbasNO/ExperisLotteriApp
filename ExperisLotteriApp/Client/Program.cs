using Client;
using Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Load appsettings from wwwroot
using var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// Choose config file based on environment
var configFile = builder.HostEnvironment.IsDevelopment()
    ? "appsettings.Development.json"
    : "appsettings.Production.json";

var config = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(configFile);
if (config is not null && config.TryGetValue("ApiBaseUrl", out var apiBaseUrl))
{
    builder.Services.AddScoped(sp => new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl)
    });
}
else
{
    throw new Exception("ApiBaseUrl not found in configuration.");
}

builder.Services.AddScoped<ITicketService, TicketService>();

await builder.Build().RunAsync();
