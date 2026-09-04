using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// The controller's symbol table, decoded once and held for lookup: a map from tag name to the
/// <see cref="TagDefinition"/> the controller reports for it. Names are matched
/// <b>case-insensitively</b>, because Logix tag names are.
/// The injected dictionary must be keyed with <see cref="TagName.CaseInsensitiveComparer"/>.
/// </summary>
internal sealed class TagDefinitions(IReadOnlyDictionary<TagName, TagDefinition> tagDefinitions)
{
    /// <summary>The controller's declaration for <paramref name="tagName"/>, or <c>null</c> when absent.</summary>
    public TagDefinition? Lookup(TagName tagName) =>
        tagDefinitions.TryGetValue(tagName, out var declaration) ? declaration : null;
}
