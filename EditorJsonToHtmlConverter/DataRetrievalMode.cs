namespace EditorJsonToHtmlConverter;

/// <summary>
/// Controls whether the renderer outputs embedded data or GUID references for leaflet-map blocks.
/// </summary>
public enum DataRetrievalMode
{
    /// <summary>
    /// Outputs fully resolved data as a child script element. Used by management-www for internal rendering.
    /// </summary>
    Embedded,

    /// <summary>
    /// Outputs data-* attributes with GUID references. Used by community-gateway-www for external rendering.
    /// </summary>
    Reference
}
