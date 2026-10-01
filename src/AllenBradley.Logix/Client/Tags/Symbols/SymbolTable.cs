using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.TagsListing;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;

/// <summary>
/// The controller's symbol table, decoded once and held for lookup: every tag-listing and every
/// template that is used by one of those tags. Logix tag names are case-insensitive, and so is the lookup.
/// </summary>
internal sealed class SymbolTable(
    IReadOnlyList<ListedTag> listedTags,
    IReadOnlyList<TemplateDefinition> usedTemplates)
{
    private readonly Dictionary<TagAddress, TagDefinition> _tagsByAddress =
        listedTags.ToDictionary(tag => tag.TagAddress, tag => tag.TagDefinition, TagAddress.CaseInsensitiveComparer);

    private readonly Dictionary<TemplateId, TemplateDefinition> _templatesById =
        usedTemplates.ToDictionary(template => template.Id);

    /// <summary>
    /// What the controller declares the value <paramref name="tagPath"/> reaches to be, at the address
    /// the path renders to: the tag's own type for a tag, the member's — read off the templates, one
    /// per member passed through — for a member, and a scalar of the array's type for an element.
    /// <c>null</c> when the path stops short: the tag is not listed, a member is not in its template,
    /// a member is asked of something that has none, or an element is asked of a scalar or past its end.
    /// </summary>
    public DeclaredType? GetDeclaredTypeAtPath(TagPath tagPath) =>
        FollowPath(tagPath) is { } reached
            ? DeclaredTypeOf(reached, at: tagPath.ToTagAddress())
            : null;

    private TagDefinition? FollowPath(TagPath tagPath)
    {
        if (GetListedTag(tagPath) is not { } tag)
        {
            return null;
        }

        if (FollowMemberPath(from: tag, tagPath.UdtMemberPath) is not { } member)
        {
            return null;
        }

        return FollowElementIndex(from: member, tagPath.ArrayElementIndex);
    }

    private DeclaredType DeclaredTypeOf(TagDefinition reached, TagAddress at) =>
        IdentifyPredefinedStructure(reached).At(at);

    private TagDefinition? GetListedTag(TagPath tagPath) =>
        _tagsByAddress.TryGetValue(tagPath.RootTagAddress, out var tag) ? tag : null;

    // Each member is looked up in the template of the type reached so far, so the walk stops at
    // the first member the templates do not know.
    private TagDefinition? FollowMemberPath(TagDefinition from, UdtMemberPath? path)
    {
        var reached = from;

        foreach (var memberName in path?.Members ?? [])
        {
            if (MemberOf(reached, memberName) is not { } member)
            {
                return null;
            }

            reached = member;
        }

        return reached;
    }

    // An array, an atomic and a structure whose template the controller does not serve have no
    // members to find.
    private TagDefinition? MemberOf(TagDefinition holder, UdtMemberName name) =>
        holder.DimensionCount.IsScalar
            ? TemplateNamed(holder.TemplateId)?.FindMember(name)?.TagDefinition
            : null;

    // The listing names no element, so the subscript is read against the array it is an element of.
    private static TagDefinition? FollowElementIndex(TagDefinition from, ElementIndex? index)
    {
        if (index is null)
        {
            return from;
        }

        return !from.DimensionCount.IsScalar && index.Value.IsWithin(from.ElementCount)
            ? from.OfOneElement()
            : null;
    }

    // The listing names a structure by template id alone, so a structure arrives with no data type.
    // STRING, TIMER and COUNTER are the structures this dataport reads as a value, and the template says which.
    private TagDefinition IdentifyPredefinedStructure(TagDefinition declared) =>
        TemplateNamed(declared.TemplateId) switch
        {
            { StringCapacity: { } capacity } => declared.AsStringOf(capacity),
            { IsTimer: true } => declared.AsTimer(),
            { IsCounter: true } => declared.AsCounter(),
            _ => declared,
        };

    private TemplateDefinition? TemplateNamed(TemplateId? templateId) =>
        templateId is { } id && _templatesById.TryGetValue(id, out var template) ? template : null;
}
