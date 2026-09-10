using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Constructs the <see cref="TagDefinition"/>s that stand in for what a controller's symbol table
/// reports, so a suite about verification or tag lifetime need not browse one.
/// </summary>
internal static class TagDefinitionTestDataFactory
{
    internal static readonly DimensionCount Scalar = new(0);

    internal static readonly ElementCount OneElement = new(1);

    internal static readonly ElementCount TenElements = new(10);

    /// <summary>What the controller reports for a DINT tag.</summary>
    internal static TagDefinition DefaultAtomicTagDefinition() =>
        new(DefaultTagName, LogixTypeKind.Atomic, AllenBradleyDataType.Dint, MaxLength: null, Scalar, OneElement);

    /// <summary>What the controller reports for a built-in STRING tag: a scalar structure of 82 characters.</summary>
    internal static TagDefinition DefaultStringTagDefinition() =>
        new(DefaultTagName, LogixTypeKind.Structure, AllenBradleyDataType.String, StringMaxLength.Standard, Scalar,
            OneElement);

    /// <summary>What the controller reports for a ten-element one-dimensional INT array tag.</summary>
    internal static TagDefinition DefaultIntArrayTagDefinition() =>
        new(DefaultTagName, LogixTypeKind.Atomic, AllenBradleyDataType.Int, MaxLength: null,
            DimensionCount.OneDimensional, TenElements);
}
