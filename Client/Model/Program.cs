using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PersonalWebsite.Client;
using PersonalWebsite.Shared.Contact;
using PersonalWebsite.Shared.CV;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddHttpClient("local",
    (h) => h.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddHttpClient("api",
    (h) => h.BaseAddress = new Uri("http://localhost:7071"));

// TODO: Replace this with a proper http rest service
builder.Services.AddHttpClient<ICVService, JsonCVService>("local");

builder.Services.AddOptions<HttpContactOptions>().Bind(builder.Configuration.GetSection(nameof(HttpContactOptions))).ValidateDataAnnotations();
builder.Services.AddHttpClient<IContactService, HttpContactService>("api");

await builder.Build().RunAsync();
