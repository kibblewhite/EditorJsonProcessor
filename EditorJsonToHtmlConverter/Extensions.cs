namespace EditorJsonToHtmlConverter;

public static class Extensions
{
    /// <summary>
    /// Adds the scoped services required for processing EditorJS JSON output and rendering HTML content.
    /// </summary>
    /// <remarks>The instance EjsHtmlRenderer can now be injected into classes.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddScopedEditorJsonProcessorServices(this IServiceCollection services)
    {
        services.AddScoped<HtmlRenderer>();
        services.AddScoped<EjsHtmlRenderer>();
        return services;
    }
}
