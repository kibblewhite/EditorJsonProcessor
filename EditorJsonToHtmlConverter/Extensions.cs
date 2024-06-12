namespace EditorJsonToHtmlConverter;

public static class Extensions
{
    public static IServiceCollection AddScopedEditorJsonProcessorServices(this IServiceCollection services)
    {
        services.AddScoped<HtmlRenderer>();
        services.AddScoped<EjsHtmlRenderer>();
        return services;
    }
}
