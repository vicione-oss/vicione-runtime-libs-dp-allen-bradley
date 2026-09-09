using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// The whole type rule, in one function: the controller's TagDefinition read against what a converter
// expects the tag to be. Configuration verification is the only caller, and it runs once per connect
// (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md) — reads and writes trust the
// verdict it reached rather than re-reaching it per operation.
//
// It is generic over the converters because the expected side is data: a kind, a data type, a rank and an
// extent, which is the same shape the controller reports the actual side in. An elementary type, a STRING
// and an array differ in the constants they supply, not in how they are compared.
internal static class LogixTypeComparison
{
    // Rank, then kind, then type, then capacity or element count. Rank leads because a shape that
    // disagrees makes everything after it meaningless in either direction: a scalar configured against an
    // array holds the wrong number of values whatever they are, and an array configured against a scalar
    // reads a tag that has nothing after its first element.
    // A tag with no declaration has nothing to compare, so it reports None rather than a contradiction;
    // the verifier reports that tag as absent from the controller before it ever asks here.
    internal static LogixTypeMismatch Compare(IDataPointConverter converter, ResolvedDataPoint resolved)
    {
        if (resolved.TagDefinition is not { } declaration)
        {
            return LogixTypeMismatch.None;
        }

        if (declaration.DimensionCount != converter.ExpectedDimensionCount)
        {
            return LogixTypeMismatch.Rank;
        }

        if (declaration.Kind != converter.ExpectedKind)
        {
            // Named from what the controller reports, not from what was configured: a structure where an
            // elementary type is configured, and Atomic for the inverse.
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

    // Only a shape whose extent is configured has anything to say here. A scalar answers null, and the
    // count of one the controller reports for it is nothing to hold that against.
    private static LogixTypeMismatch CompareElementCount(
        IDataPointConverter converter, ILogixDataPoint dataPoint, TagDefinition declaration) =>
        converter.ElementCountFor(dataPoint) is not { } elementCount
        || declaration.ElementCount == elementCount
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.ElementCount;

    // Both sides are character capacities, so this is the comparison the configuration is actually
    // written in: a STRING (82) configured onto a STRING_20 (20) is the error it exists to catch. The
    // data type is not compared alongside it because there is only one structured type — every structure
    // decodes as a String (see TagsDecoder), so within this branch both sides always agree on it.
    private static LogixTypeMismatch CompareCapacity(
        IDataPointConverter converter, ILogixDataPoint dataPoint, TagDefinition declaration) =>
        declaration.MaxLength == converter.MaxLengthFor(dataPoint)
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.StringCapacity;
}
