using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EditorJsonToHtmlConverter;
using BlazorApp.Client;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScopedEditorJsonProcessorServices(opts =>
    // Tile URL used by every map block produced by the renderer. Hits the
    // tile proxy on BlazorApp.Server (which caches and forwards to pull-pmtiles).
    // opts.TileUrlTemplate = "/tiles/{z}/{x}/{y}.mvt");
    opts.TileUrlTemplate = "https://pull-pmtiles.internal.zone/tiles/{z}/{x}/{y}.mvt");

await builder.Build().RunAsync();
