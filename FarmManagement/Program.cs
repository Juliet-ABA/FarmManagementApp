using FarmManagement;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var apiBaseAddress = builder.Configuration["ApiBaseAddress"];

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

builder.Services.AddMudServices();

await builder.Build().RunAsync();
