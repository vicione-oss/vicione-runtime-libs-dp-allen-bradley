using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80;

/// <summary>
/// What a round-trip suite expects the controller to declare the value at its data point's address to
/// be, built whole so it can be compared as one record.
/// </summary>
internal static class ExpectedDeclaredTypes
{
    /// <summary>What the controller must declare an elementary scalar of <paramref name="dataType"/> to be.</summary>
    internal static DeclaredType AtomicScalar(ILogixDataPoint dataPoint, AllenBradleyDataType dataType) =>
        new(dataPoint.TagAddress, dataType, MaxLength: null, DimensionCount.Scalar, new ElementCount(1));

    /// <summary>
    /// What the controller must declare a one-dimensional array of <paramref name="elementCount"/>
    /// elementary <paramref name="dataType"/> values to be.
    /// </summary>
    internal static DeclaredType AtomicArray(
        ILogixDataPoint dataPoint, AllenBradleyDataType dataType, ElementCount elementCount) =>
        new(dataPoint.TagAddress, dataType, MaxLength: null, DimensionCount.OneDimensional, elementCount);

    /// <summary>
    /// What the controller must declare a <c>STRING</c> tag holding <paramref name="maxLength"/>
    /// characters to be: a structure on the wire, a scalar in this model.
    /// </summary>
    internal static DeclaredType StringScalar(ILogixDataPoint dataPoint, StringMaxLength maxLength) =>
        new(dataPoint.TagAddress, AllenBradleyDataType.String, maxLength, DimensionCount.Scalar, new ElementCount(1));

    /// <summary>
    /// What the controller must declare a <c>TIMER</c> tag to be: a structure on the wire, a scalar in this
    /// model, declared at the timer's address rather than at the <c>.ACC</c> its handle reaches.
    /// </summary>
    internal static DeclaredType TimerScalar(ILogixDataPoint dataPoint) =>
        new(dataPoint.TagAddress, AllenBradleyDataType.Timer, MaxLength: null, DimensionCount.Scalar, new ElementCount(1));

    /// <summary>
    /// What the controller must declare a <c>COUNTER</c> tag to be: a structure on the wire, a scalar in
    /// this model, declared at the counter's address rather than at the <c>.ACC</c> its handle reaches.
    /// </summary>
    internal static DeclaredType CounterScalar(ILogixDataPoint dataPoint) =>
        new(dataPoint.TagAddress, AllenBradleyDataType.Counter, MaxLength: null, DimensionCount.Scalar, new ElementCount(1));
}
