namespace EditorJsonToHtmlConverter;

/// <summary>
/// Controls whether the renderer outputs embedded data or GUID references for leaflet-map blocks.
/// </summary>
public enum DataRetrievalMode
{
    /// <summary>
    /// Outputs fully resolved data as a child <c>&lt;script type="application/json"&gt;</c> element.
    /// The consuming application resolves all GUID references to full data objects before rendering.
    /// The client reads the self-contained JSON and renders immediately without further API calls.
    /// </summary>
    Embedded,

    /// <summary>
    /// Outputs <c>data-*</c> attributes with GUID references and map configuration on the container element.
    /// A client-side JS viewer discovers the containers, reads the attributes, and fetches full data
    /// from API endpoints at runtime using the locale specified in <c>data-locale</c>.
    /// </summary>
    Reference
}
