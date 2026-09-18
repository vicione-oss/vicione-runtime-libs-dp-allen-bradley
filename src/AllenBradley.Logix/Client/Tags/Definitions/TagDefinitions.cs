using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;

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
    /// <summary>
    /// The controller's declaration nearest the value <paramref name="tagPath"/> reaches: the tag's own
    /// for a tag or an element of it, and the member's — read off the templates, one per member passed
    /// through — for a member. The listing names no element, so a subscript is never looked up; the
    /// comparison reads it against the array. <c>null</c> when the tag or a member on the way is not there.
    /// </summary>
    public TagDefinition? Lookup(TagPath tagPath)
    {
        if (!tagDefinitions.TryGetValue(tagPath.TagDefinitionAddress, out var tag))
        {
            return null;
        }

        TagDefinition? declaration = tag;
        foreach (var name in tagPath.UdtMemberPath?.Members ?? [])
        {
            if (declaration is not { } holder)
            {
                break;
            }

            declaration = MemberOf(holder, name);
        }

        return declaration;
    }

    /// <summary>
    /// What the listing would report for the member <paramref name="name"/> of <paramref name="holder"/>
    /// if it listed members: found in the template the holder names, addressed behind the holder as the
    /// template spells it. A structure member is a string, as every structure is in the listing, with
    /// its template's capacity where the listing reads an element length. An array, an atomic and a
    /// structure whose template the controller does not serve have no members to find.
    /// </summary>
    private TagDefinition? MemberOf(TagDefinition holder, UdtMemberName name)
    {
        if (!holder.DimensionCount.IsScalar || TemplateNamed(holder.TemplateId)?.FindMember(name) is not { } member)
        {
            return null;
        }

        return new TagDefinition(
            new TagAddress($"{holder.TagAddress.Value}.{member.Name.Value}"),
            member.TemplateId is null ? member.DataType : AllenBradleyDataType.String,
            member.TemplateId,
            TemplateNamed(member.TemplateId)?.StringCapacity,
            member.DimensionCount,
            member.ElementCount);
    }

    private TemplateDefinition? TemplateNamed(TemplateId? templateId) =>
        templateId is { } id && templateDefinitions.TryGetValue(id, out var template) ? template : null;
}
