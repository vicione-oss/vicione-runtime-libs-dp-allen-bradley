using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// The controller's symbol table, decoded once and held for lookup. Logix tag names are
/// case-insensitive, so the injected dictionary must be keyed with
/// <see cref="TagAddress.CaseInsensitiveComparer"/>.
/// </summary>
internal sealed class TagDefinitions(IReadOnlyDictionary<TagAddress, TagDefinition> tagDefinitions)
{
    /// <summary>The controller's declaration for <paramref name="tagAddress"/>, or <c>null</c> when absent.</summary>
    public TagDefinition? Lookup(TagAddress tagAddress) =>
        tagDefinitions.TryGetValue(tagAddress, out var declaration) ? declaration : null;
}
