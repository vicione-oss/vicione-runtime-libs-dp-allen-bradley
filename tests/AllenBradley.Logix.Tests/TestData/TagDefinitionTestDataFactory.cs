using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.DeclaredTypeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Constructs the <see cref="TagDefinition"/>s the symbol-table walk holds for a listed tag or a template
/// member, so a suite about the lookup or the decoders need not browse a controller.
/// </summary>
internal static class TagDefinitionTestDataFactory
{
    /// <summary>The template a STRING tag names — the id in <c>TagsListingTestDataFactory.StructureSymbolType</c>.</summary>
    internal static readonly TemplateId DefaultTemplateId = new(0x123);

    /// <summary>What the controller declares a DINT tag to be.</summary>
    internal static TagDefinition DefaultAtomicTagDefinition() =>
        new(AllenBradleyDataType.Dint, TemplateId: null, MaxLength: null, Scalar, OneElement);

    /// <summary>What the controller declares a built-in STRING tag to be: a scalar structure of 82 characters.</summary>
    internal static TagDefinition DefaultStringTagDefinition() =>
        new(AllenBradleyDataType.String, DefaultTemplateId, StringMaxLength.Standard, Scalar, OneElement);

    /// <summary>
    /// What a structure is before the lookup has read its template: a structure, naming the template
    /// that says which kind.
    /// </summary>
    internal static TagDefinition DefaultStructureTagDefinition() =>
        new(AllenBradleyDataType.Structure, DefaultTemplateId, MaxLength: null, Scalar, OneElement);

    /// <summary>What the controller declares a ten-element one-dimensional INT array tag to be.</summary>
    internal static TagDefinition DefaultIntArrayTagDefinition() =>
        new(AllenBradleyDataType.Int, TemplateId: null, MaxLength: null, DimensionCount.OneDimensional, TenElements);
}
