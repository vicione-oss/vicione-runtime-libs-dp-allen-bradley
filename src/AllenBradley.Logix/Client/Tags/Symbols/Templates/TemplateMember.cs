using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;

/// <summary>
/// One member of a structure as its template describes it: where it sits and what it is declared to
/// be, in the same <see cref="TagDefinition"/> a tag is, so a member and a tag of the same type read
/// alike. A member is not a tag (CONTEXT.md, "Member"): it has no address of its own, and it is
/// reached through the tag that holds the structure.
/// </summary>
/// <param name="Name">The member's name, as the template spells it.</param>
/// <param name="Offset">Where the member's bytes start within the structure instance.</param>
/// <param name="TagDefinition">
/// What the member holds. A structure member names its own template there; what that template is
/// called is the template's <see cref="TemplateDefinition.Name"/>. A member is at most a
/// one-dimensional array.
/// </param>
/// <param name="BitPosition">Which bit of the host byte at <paramref name="Offset"/> a <c>BOOL</c> member is; <c>null</c> for every other member.</param>
internal readonly record struct TemplateMember(
    UdtMemberName Name,
    MemberOffset Offset,
    TagDefinition TagDefinition,
    BitPosition? BitPosition);
