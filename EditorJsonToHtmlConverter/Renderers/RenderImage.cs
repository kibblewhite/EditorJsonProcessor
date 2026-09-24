namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders an image with an optional caption and presentation flags.
/// </summary>
/// <remarks>
/// <para>
/// The renderer references an image by URL; it neither uploads nor hosts one, so the URL must already
/// resolve for the reader.
/// </para>
/// <para>
/// <c>url</c> (required) — the image source. <c>caption</c> (optional) — used as the image's alternative
/// text AND shown as a centred line beneath it, so it should describe the image in plain text; markup would
/// appear literally in the alternative text. <c>withBorder</c> (optional, default false) — draws a thin
/// border. <c>withBackground</c> (optional, default false) — centres the image horizontally; no background is
/// drawn. <c>stretched</c> (optional, default false) — spans the full content width.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "id": "i1a2b3c4d5",
///   "type": "image",
///   "data": { "url": "https://example.test/venue.jpg", "caption": "The main hall set for dinner", "stretched": true }
/// }
/// </code>
/// </example>
public sealed class RenderImage : IBlockRenderer
{
    /// <inheritdoc />
    public static SupportedRenderers BlockType => SupportedRenderers.Image;

    public static void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block)
    {
        string id = block.Id;
        string? url = block.Data.Url;
        string? caption = block.Data.Caption;
        bool withBorder = block.Data.WithBorder ?? false;
        bool withBackground = block.Data.WithBackground ?? false;
        bool stretched = block.Data.Stretched ?? false;

        // Render image
        render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "img");
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "id", id);

        EditorJsStylingMap? css = render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Image && item.Id == id);
        css ??= render_tree_builder.StylingMap.FirstOrDefault(item => item.Type == SupportedRenderers.Image && item.Id == null);

        if (css is not null)
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "class", css.Style);
        }

        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "src", url);
        render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "alt", caption);

        StringBuilder styleBuilder = new();

        if (withBorder)
        {
            styleBuilder.Append("border: 1px solid #ddd; ");
        }

        if (withBackground)
        {
            styleBuilder.Append("margin: 0 auto; ");
        }

        if (stretched)
        {
            styleBuilder.Append("width: 100%; ");
        }

        string style = styleBuilder.ToString().Trim();

        if (!string.IsNullOrWhiteSpace(style))
        {
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "style", style);
        }

        render_tree_builder.Builder.CloseElement(); // Close the img element

        // Render caption
        if (!string.IsNullOrEmpty(caption))
        {
            render_tree_builder.Builder.OpenElement(render_tree_builder.SequenceCounter, "p");
            render_tree_builder.Builder.AddAttribute(render_tree_builder.SequenceCounter, "style", "text-align: center;");
            render_tree_builder.Builder.AddMarkupContent(render_tree_builder.SequenceCounter, caption);
            render_tree_builder.Builder.CloseElement(); // Close the p element
        }
    }
}
