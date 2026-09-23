namespace EditorJsonToHtmlConverter.Renderers;

/// <summary>
/// Renders one Editor.js block type into the render tree. Exactly one implementation exists per
/// <see cref="SupportedRenderers"/> member, and <see cref="BlockType"/> is the link between the two.
/// </summary>
/// <remarks>
/// <para>
/// <b>Documentation contract — read this before adding a renderer.</b> The XML documentation on an
/// implementing class is not commentary. It is emitted into <c>EditorJsonToHtmlConverter.xml</c>, shipped
/// beside the assembly, and read <i>at runtime</i> by downstream consumers: the assistants MCP server
/// reflects every <see cref="IBlockRenderer"/>, reads <see cref="BlockType"/>, and serves these elements to
/// a reasoning model so it can author block JSON unaided. A renderer documented to this contract is
/// therefore understood everywhere with no further change anywhere; one that is not appears with its block
/// type alone and the model has to guess the rest.
/// </para>
/// <para>Every implementation carries, in this order:</para>
/// <list type="number">
/// <item><description>
/// <c>&lt;summary&gt;</c> — a single present-tense sentence naming what the block renders.
/// </description></item>
/// <item><description>
/// <c>&lt;remarks&gt;</c> — when to choose this block over its neighbours, then every <c>data</c> field the
/// renderer reads: the JSON field name as it appears on the wire, whether it is required or optional, and
/// what the renderer does when it is absent. Describe observable behaviour, never implementation detail.
/// </description></item>
/// <item><description>
/// <c>&lt;example&gt;</c> wrapping <c>&lt;code&gt;</c> — one complete, minimal, valid block object,
/// including <c>id</c> and <c>type</c>. This is copied verbatim by consumers as the worked example, so it
/// must parse and must render.
/// </description></item>
/// </list>
/// <para>
/// Adding a block type is then: add the <see cref="SupportedRenderers"/> member, add the renderer declaring
/// its <see cref="BlockType"/>, document it to the three points above, and add its dispatch case. Nothing
/// downstream needs editing.
/// </para>
/// </remarks>
public interface IBlockRenderer
{
    /// <summary>
    /// The Editor.js block type this renderer handles. A block whose <c>type</c> string parses
    /// case-insensitively to this value is rendered by this implementation.
    /// </summary>
    /// <remarks>
    /// Declared rather than inferred from the class name so the mapping is compiler-enforced: a new renderer
    /// cannot be added without stating which block type it serves, and consumers reflecting this interface
    /// never have to parse type names.
    /// </remarks>
    static abstract SupportedRenderers BlockType { get; }

    /// <summary>Writes this block into the render tree.</summary>
    /// <param name="render_tree_builder">The builder the block is written into.</param>
    /// <param name="block">The block being rendered, carrying its identifier and its <c>data</c> payload.</param>
    static abstract void Render(CustomRenderTreeBuilder render_tree_builder, EditorJsBlock block);
}
