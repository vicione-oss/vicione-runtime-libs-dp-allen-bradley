using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// The whole type rule in one function: the controller's <c>TagDefinition</c> read against what a
/// converter expects the tag to be, once per connect
/// (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md).
/// </summary>
internal static class LogixTypeComparison
{
    internal static LogixTypeMismatch Compare(IDataPointConverter converter, ResolvedDataPoint resolved)
    {
        // An absent declaration has nothing to contradict; the verifier reports that tag as missing.
        if (resolved.TagDefinition is not { } declaration)
        {
            return LogixTypeMismatch.None;
        }

        // Rank leads: a shape that disagrees makes every comparison after it meaningless either way.
        if (declaration.DimensionCount != converter.ExpectedDimensionCount)
        {
            return LogixTypeMismatch.Rank;
        }

        if (declaration.Kind != converter.ExpectedKind)
        {
            return declaration.Kind is LogixTypeKind.Structure
                ? LogixTypeMismatch.Structure
                : LogixTypeMismatch.Atomic;
        }

        return declaration.Kind is LogixTypeKind.Atomic
            ? CompareAtomicType(converter, resolved.DataPoint, declaration)
            : CompareCapacity(converter, resolved.DataPoint, declaration);
    }

    private static LogixTypeMismatch CompareAtomicType(
        IDataPointConverter converter, ILogixDataPoint dataPoint, TagDefinition declaration) =>
        declaration.DataType == converter.ExpectedDataType
            ? CompareElementCount(converter, dataPoint, declaration)
            : LogixTypeMismatch.AtomicType;

    // A scalar configures no extent and answers null, and the count of one the controller reports for it
    // is nothing to hold that against.
    private static LogixTypeMismatch CompareElementCount(
        IDataPointConverter converter, ILogixDataPoint dataPoint, TagDefinition declaration) =>
        converter.ElementCountFor(dataPoint) is not { } elementCount
        || declaration.ElementCount == elementCount
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.ElementCount;

    // No data type is compared alongside the capacity: every structure decodes as a String (see
    // TagsDecoder), so within this branch both sides always agree on it.
    private static LogixTypeMismatch CompareCapacity(
        IDataPointConverter converter, ILogixDataPoint dataPoint, TagDefinition declaration) =>
        declaration.MaxLength == converter.MaxLengthFor(dataPoint)
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.StringCapacity;
}
