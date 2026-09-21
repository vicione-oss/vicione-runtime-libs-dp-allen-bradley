using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagDefinitionTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Constructs decoded <see cref="TemplateDefinition"/>s and their members directly, without the bytes
/// <see cref="TemplateTestDataFactory"/> lays out, so a suite about the lookup or the string decision
/// need not run the decoder.
/// </summary>
internal static class TemplateDefinitionTestDataFactory
{
    internal static readonly StructureHandle DefaultStructureHandle = new(0xABCD);

    internal static readonly StructureSize DefaultStructureSize = new(4);

    internal static TemplateDefinition Template(TemplateId id, string name, params TemplateMember[] members) =>
        new(id, new TemplateName(name), DefaultStructureHandle, DefaultStructureSize, members);

    /// <summary>A scalar member of an atomic type, at offset zero.</summary>
    internal static TemplateMember AtomicMember(string name, AllenBradleyDataType dataType) =>
        new(new UdtMemberName(name), new MemberOffset(0), DefaultAtomicTagDefinition() with { DataType = dataType },
            BitPosition: null);

    /// <summary>A scalar member that is itself a structure, as the decoder leaves it: no data type, its template named.</summary>
    internal static TemplateMember StructureMember(string name, TemplateId templateId) =>
        new(new UdtMemberName(name), new MemberOffset(0), DefaultStructureTagDefinition() with { TemplateId = templateId },
            BitPosition: null);

    /// <summary><paramref name="element"/> as a one-dimensional array member of <paramref name="elementCount"/> elements.</summary>
    internal static TemplateMember ArrayMemberOf(TemplateMember element, uint elementCount) =>
        element with
        {
            TagDefinition = element.TagDefinition with
            {
                DimensionCount = DimensionCount.OneDimensional,
                ElementCount = new ElementCount(elementCount),
            },
        };
}
