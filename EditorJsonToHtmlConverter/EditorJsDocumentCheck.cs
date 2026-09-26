namespace EditorJsonToHtmlConverter;

/// <summary>
/// How far <see cref="Extensions.EditorJsBlocksExtensions.IsEditorJsDocument(string, EditorJsDocumentCheck)"/> checks a
/// value. Each level includes every check of the levels before it. Every level parses the whole value, so the cost of
/// that parse is shared; the levels differ in what is checked once it has been parsed.
/// </summary>
public enum EditorJsDocumentCheck
{
    /// <summary>
    /// JSON that parses to an object carrying a <c>blocks</c> array. The blocks themselves are not looked at. The
    /// cheapest level, for sweeping many stored values that were checked when they were written.
    /// </summary>
    Envelope,

    /// <summary>
    /// The envelope, and every field has the kind the Editor.js output format gives it: each block is an object with a
    /// non-empty string <c>type</c> and an object <c>data</c>; <c>time</c> is a number, <c>version</c> a string, a block's
    /// <c>id</c> a non-empty string and its <c>tunes</c> an object, whenever they are present. The fields Editor.js
    /// treats as optional stay optional. Walks every block, but only over the already-parsed JSON, so it costs little
    /// more than <see cref="Envelope"/>; for accepting a document from outside before storing it.
    /// </summary>
    Structure,

    /// <summary>
    /// The structure, and what this library needs to render it: <c>time</c> (a whole number of milliseconds),
    /// <c>version</c> and every block's <c>id</c> present, ids unique, every <c>type</c> naming a
    /// <see cref="SupportedRenderers"/> block, and the document deserialising into <see cref="EditorJsBlocks"/> — which
    /// checks every <c>data</c> field the renderers read has the type they read it as. The most expensive level, as it
    /// builds the model; for curating a single document before it is saved or shown.
    /// </summary>
    Renderable
}
