using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// The controller's symbol table, decoded once and held for lookup: a map from tag name to the
/// <see cref="TagDefinition"/> the controller reports for it. Names are matched
/// <b>case-insensitively</b>, because Logix tag names are.
/// </summary>
/// <remarks>
/// This is the libplctag-free equivalent of the S7 symbol tree: the browse fills it once at connect,
/// and every verification resolves against it in memory, with no further device round trip. The
/// case-insensitive match is the injected dictionary's — key it with
/// <see cref="TagName.CaseInsensitiveComparer"/>.
/// </remarks>
internal sealed class TagDefinitions(IReadOnlyDictionary<TagName, TagDefinition> declarationsByTagName)
{
    /// <summary>The controller's declaration for <paramref name="tagName"/>, or <c>null</c> when absent.</summary>
    public TagDefinition? Lookup(TagName tagName) =>
        declarationsByTagName.TryGetValue(tagName, out var declaration) ? declaration : null;

    /// <summary>Pairs <paramref name="dataPoint"/> with the controller's declaration for its tag.</summary>
    public ResolvedDataPoint Resolve(ILogixDataPoint dataPoint) =>
        new(dataPoint, Lookup(dataPoint.TagName));
}
