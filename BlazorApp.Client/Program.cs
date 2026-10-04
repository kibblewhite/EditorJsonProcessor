using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EditorJsonToHtmlConverter;
using BlazorApp.Client;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScopedEditorJsonProcessorServices(opts =>
    // Tile URL used by every map block produced by the renderer. Fetches straight
    // from the Protomaps API (free, non-commercial key, restricted to allowed origins).
    // To go through the caching tile proxy on BlazorApp.Server instead, use:
    // opts.TileUrlTemplate = "/tiles/{z}/{x}/{y}.mvt");
    opts.TileUrlTemplate = "https://api.protomaps.com/tiles/v4/{z}/{x}/{y}.mvt?key=321dc2abf8362740");

await builder.Build().RunAsync();
