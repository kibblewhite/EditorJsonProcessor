namespace EditorJsonToHtmlConverter.Tests;

/// <summary>
/// Pins the <c>wrap</c> lookup: each wrap type writes its name in lower case, a wrap value resolves in any case, and
/// anything else - including the empty wrap type's own name - does not.
/// </summary>
[TestClass]
public sealed class TextWrapTypeLookupTests
{
    [TestMethod]
    [DataRow(TextWrapType.Text, "text")]
    [DataRow(TextWrapType.Custom, "custom")]
    [DataRow(TextWrapType.Title, "title")]
    [DataRow(TextWrapType.Synopsis, "synopsis")]
    public void A_wrap_type_writes_its_wrap_tag_and_reads_it_back(TextWrapType wrap_type, string expected_wrap)
    {
        Assert.AreEqual(expected_wrap, wrap_type.ToWrap());
        Assert.IsTrue(TextWrapTypeLookup.TryGetWrap(expected_wrap.ToUpperInvariant(), out TextWrapType resolved));
        Assert.AreEqual(wrap_type, resolved);
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow(" title ")]
    [DataRow("empty")]
    [DataRow("")]
    [DataRow(null)]
    [DataRow("no-such-wrap")]
    public void Anything_but_a_wrap_tag_does_not_resolve(string? wrap)
    {
        Assert.IsFalse(TextWrapTypeLookup.TryGetWrap(wrap, out TextWrapType wrap_type));
        Assert.AreEqual(TextWrapType.Empty, wrap_type);
    }

    [TestMethod]
    public void The_empty_wrap_type_writes_nothing()
        => Assert.AreEqual(string.Empty, TextWrapType.Empty.ToWrap());
}
