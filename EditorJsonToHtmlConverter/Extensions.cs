namespace EditorJsonToHtmlConverter;

public static class Extensions
{
    /// <summary>
    /// Gets an empty <see cref="JsonObject"/> instance representing an Editor.js object.
    /// </summary>
    public static JsonObject EmptyEditorJsObject => JsonNode.Parse(EmptyEditorJsString)?.AsObject() ?? [];

    /// <summary>
    /// Gets an empty JSON string representation for an editor, including a timestamp, empty blocks, and a default
    /// version.
    /// </summary>
    /// <remarks>This property is useful for initializing or resetting editor content to a default empty
    /// state. The timestamp is generated based on the current UTC time.</remarks>
    public static string EmptyEditorJsString => $"{{ \"time\": {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}, \"blocks\": [], \"version\": \"0.0.0\" }}";

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
