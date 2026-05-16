namespace EditorJsonToHtmlConverter;

public static class BuilderExtensions
{
    /// <summary>
    /// Adds the scoped services required for processing EditorJS JSON output and rendering HTML content.
    /// </summary>
    /// <remarks>
    /// The DI-resolved <see cref="EjsHtmlRenderer"/> will use the default <see cref="EditorJsonProcessorOptions"/>
    /// (no tile URL configured). Use the <c>configure</c> overload to supply renderer-level options
    /// such as the map <see cref="EditorJsonProcessorOptions.TileUrlTemplate"/>.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddScopedEditorJsonProcessorServices(this IServiceCollection services) =>
        AddScopedEditorJsonProcessorServices(services, configure: null);

    /// <summary>
    /// Adds the scoped services required for processing EditorJS JSON output and rendering HTML content,
    /// with an opportunity to configure renderer-level options.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddScopedEditorJsonProcessorServices(opts =>
    /// {
    ///     opts.TileUrlTemplate = "https://your-cdn.example/tiles/{z}/{x}/{y}.mvt";
    /// });
    /// </code>
    /// </example>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configure">Optional callback to configure <see cref="EditorJsonProcessorOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddScopedEditorJsonProcessorServices(
        this IServiceCollection services,
        Action<EditorJsonProcessorOptions>? configure)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddScoped<HtmlRenderer>();

        // Factory registration so the DI-resolved EjsHtmlRenderer picks up the
        // configured TileUrlTemplate. Manual construction (`new EjsHtmlRenderer(...)`)
        // bypasses this factory — callers using that path must pass tile_url_template
        // explicitly.
        services.AddScoped(sp =>
        {
            HtmlRenderer html_renderer = sp.GetRequiredService<HtmlRenderer>();
            IOptions<EditorJsonProcessorOptions>? options = sp.GetService<IOptions<EditorJsonProcessorOptions>>();
            return new EjsHtmlRenderer(
                html_renderer,
                tile_url_template: options?.Value.TileUrlTemplate);
        });

        return services;
    }
}
