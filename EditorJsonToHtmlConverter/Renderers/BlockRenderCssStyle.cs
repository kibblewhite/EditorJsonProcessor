namespace EditorJsonToHtmlConverter.Renderers;

public sealed class BlockRenderCssStyle
{
    // todo (2024-20-09|kibble) Add in optional parameters (default null) for things like header which matches on level for example...
    public static EditorJsStylingMap? BuildEditorJsStylings(CustomRenderTreeBuilder render_tree_builder, SupportedRenderers render_type, string? id, int? level = null)
    {
        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == render_type && item.Level == level && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == render_type && item.Level == level && item.Id == null);
        return css;
    }
}
