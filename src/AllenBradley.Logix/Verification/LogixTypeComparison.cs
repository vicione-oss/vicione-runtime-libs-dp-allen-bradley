using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// The whole type rule in one function: the controller's <c>TagDefinition</c> read against what the
/// configured data point says the tag is, once per connect
/// (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md).
/// </summary>
internal static class LogixTypeComparison
{
    internal static LogixTypeMismatch Compare(ResolvedDataPoint resolved)
    {
        // An absent declaration has nothing to contradict; the verifier reports that tag as missing.
        if (resolved.TagDefinition is not { } declaration)
        {
            return LogixTypeMismatch.None;
        }

        return resolved.DataPoint.TagPath.Element is { } index
            ? CompareElement(resolved.DataPoint, declaration, index)
            : CompareDeclaration(resolved.DataPoint, declaration);
    }

    // The listing never names an element, so the declaration is the array's; the subscript is read
    // against it first, and the element itself is then compared as the scalar it is.
    private static LogixTypeMismatch CompareElement(
        ILogixDataPoint dataPoint, TagDefinition arrayDeclaration, ElementIndex index)
    {
        if (arrayDeclaration.DimensionCount.IsScalar)
        {
            return LogixTypeMismatch.ElementOfScalar;
        }

        return index.IsWithin(arrayDeclaration.ElementCount)
            ? CompareDeclaration(dataPoint, arrayDeclaration.OfOneElement())
            : LogixTypeMismatch.ElementIndexOutOfRange;
    }

    private static LogixTypeMismatch CompareDeclaration(ILogixDataPoint dataPoint, TagDefinition declaration)
    {
        // Rank leads: a shape that disagrees makes every comparison after it meaningless either way.
        if (declaration.DimensionCount != dataPoint.DimensionCount)
        {
            return LogixTypeMismatch.Rank;
        }

        if (declaration.DataType != dataPoint.DataType)
        {
            return LogixTypeMismatch.DataType;
        }

        return dataPoint switch
        {
            StringDataPoint stringDataPoint => CompareCapacity(stringDataPoint, declaration),
            ILogixArrayDataPoint arrayDataPoint => CompareElementCount(arrayDataPoint, declaration),
            _ => LogixTypeMismatch.None,
        };
    }

    private static LogixTypeMismatch CompareCapacity(StringDataPoint dataPoint, TagDefinition declaration) =>
        declaration.MaxLength == dataPoint.MaxLength
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.StringCapacity;

    private static LogixTypeMismatch CompareElementCount(ILogixArrayDataPoint dataPoint, TagDefinition declaration) =>
        declaration.ElementCount == dataPoint.ElementCount
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.ElementCount;
}
