using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;

/// <summary>
/// What the controller's Template object reports for one structure data type, decoded from an
/// <c>@udt/&lt;id&gt;</c> read: the layout that turns a structure from a byte count into named members
/// (CONTEXT.md, "Template"). The <see cref="Id"/> of a structured tag names it by id.
/// </summary>
/// <param name="Name">The template's id, the one the tag's symbol type carries.</param>
/// <param name="Handle">The structure data type's name.</param>
/// <param name="Size">The handle a read reply of a tag of this type carries.</param>
/// <param name="Members">The bytes one instance occupies, padding included.</param>
/// <param name="Members">The members in the order the template lists them, which is declaration order.</param>
internal readonly record struct TemplateDefinition(
    TemplateId Id,
    TemplateName Name,
    StructureHandle Handle,
    StructureSize Size,
    IReadOnlyList<TemplateMember> Members)
{
    private static readonly UdtMemberName StringDataMember = new("DATA");

    private const string TimerName = "TIMER";

    /// <summary>
    /// The member <paramref name="name"/> spells, matched the way the controller matches names —
    /// ordinal, case-insensitive — or <c>null</c> when this structure has no such member.
    /// </summary>
    public TemplateMember? FindMember(UdtMemberName name)
    {
        foreach (var member in Members)
        {
            if (string.Equals(member.Name.Value, name.Value, StringComparison.OrdinalIgnoreCase))
            {
                return member;
            }
        }

        return null;
    }

    /// <summary>
    /// The characters this structure holds when it is a string type — the <c>n</c> of its
    /// <c>.DATA : SINT[n]</c> member — or <c>null</c> when it has no such member and is no string.
    /// </summary>
    public StringMaxLength? StringCapacity =>
        FindMember(StringDataMember)?.TagDefinition is { DataType: var type, DimensionCount.IsScalar: false } data
        && type == AllenBradleyDataType.Sint
            ? new StringMaxLength((int)data.ElementCount.Value)
            : null;

    /// <summary>
    /// Whether this is the predefined <c>TIMER</c>, told by its name, matched the way the controller
    /// matches names. The name is reserved, so no UDT can carry it.
    /// </summary>
    public bool IsTimer => string.Equals(Name.Value, TimerName, StringComparison.OrdinalIgnoreCase);
}
