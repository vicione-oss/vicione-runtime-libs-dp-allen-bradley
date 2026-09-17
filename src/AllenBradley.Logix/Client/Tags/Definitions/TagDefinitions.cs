using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// The controller's symbol table, decoded once and held for lookup: every tag by name, and every
/// template a structured tag names, by id. Logix tag names are case-insensitive, so the injected
/// dictionary must be keyed with <see cref="TagAddress.CaseInsensitiveComparer"/>.
/// </summary>
internal sealed class TagDefinitions(
    IReadOnlyDictionary<TagAddress, TagDefinition> tagDefinitions,
    IReadOnlyDictionary<TemplateId, TemplateDefinition> templateDefinitions)
{
    /// <summary>The controller's declaration for <paramref name="tagAddress"/>, or <c>null</c> when absent.</summary>
    public TagDefinition? Lookup(TagAddress tagAddress) =>
        tagDefinitions.TryGetValue(tagAddress, out var declaration) ? declaration : null;

    /// <summary>
    /// The layout of the structure <paramref name="templateId"/> names, or <c>null</c> when no tag the
    /// browse saw follows it.
    /// </summary>
    public TemplateDefinition? LookupTemplate(TemplateId templateId) =>
        templateDefinitions.TryGetValue(templateId, out var template) ? template : null;
}
