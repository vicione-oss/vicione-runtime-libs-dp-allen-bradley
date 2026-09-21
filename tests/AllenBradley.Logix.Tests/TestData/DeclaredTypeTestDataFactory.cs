using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;

/// <summary>
/// Constructs the <see cref="DeclaredType"/>s that stand in for what a controller declares the value at
/// an address to be, so a suite about verification or tag lifetime need not browse one.
/// </summary>
internal static class DeclaredTypeTestDataFactory
{
    internal static readonly DimensionCount Scalar = new(0);

    internal static readonly ElementCount OneElement = new(1);

    internal static readonly ElementCount TenElements = new(10);

    /// <summary>What the controller declares a DINT tag to be.</summary>
    internal static DeclaredType DefaultAtomicDeclaredType() =>
        new(DefaultTagAddress, AllenBradleyDataType.Dint, MaxLength: null, Scalar, OneElement);

    /// <summary>What the controller declares a built-in STRING tag to be: a scalar of 82 characters.</summary>
    internal static DeclaredType DefaultStringDeclaredType() =>
        new(DefaultTagAddress, AllenBradleyDataType.String, StringMaxLength.Standard, Scalar, OneElement);

    /// <summary>What the controller declares a ten-element one-dimensional INT array tag to be.</summary>
    internal static DeclaredType DefaultIntArrayDeclaredType() =>
        new(DefaultTagAddress, AllenBradleyDataType.Int, MaxLength: null, DimensionCount.OneDimensional, TenElements);
}
