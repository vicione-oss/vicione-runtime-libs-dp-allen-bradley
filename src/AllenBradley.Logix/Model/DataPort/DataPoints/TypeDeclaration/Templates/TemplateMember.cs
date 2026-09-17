namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;

/// <summary>
/// One member of a structure as its template describes it: where it sits and what it holds, in the
/// same vocabulary a <see cref="TagDefinition"/> speaks so a member and a tag of the same type read
/// alike. A member is not a tag (CONTEXT.md, "Member"): it has no address of its own, and it is
/// reached through the tag that holds the structure.
/// </summary>
/// <param name="Name">The member's name, as the template spells it.</param>
/// <param name="Offset">Where the member's bytes start within the structure instance.</param>
/// <param name="DataType">
/// The atomic data type the member holds. A structure member carries
/// <see cref="AllenBradleyDataType.Unknown"/> here and names its own template in <paramref name="TemplateId"/>;
/// what the template is called is that template's <see cref="TemplateDefinition.Name"/>.
/// </param>
/// <param name="TemplateId">The template of a member that is itself a structure; <c>null</c> for an atomic member.</param>
/// <param name="DimensionCount">The array rank: scalar, or <c>1</c> — a structure member is at most a one-dimensional array.</param>
/// <param name="ElementCount">The number of elements: the declared length of an array member, or <c>1</c> for a scalar.</param>
/// <param name="BitPosition">Which bit of the host byte at <paramref name="Offset"/> a <c>BOOL</c> member is; <c>null</c> for every other member.</param>
public readonly record struct TemplateMember(
    MemberName Name,
    MemberOffset Offset,
    AllenBradleyDataType DataType,
    TemplateId? TemplateId,
    DimensionCount DimensionCount,
    ElementCount ElementCount,
    BitPosition? BitPosition);
