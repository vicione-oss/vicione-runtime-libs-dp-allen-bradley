using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// The whole type rule in one function: what the controller declares a path to be, read against what
/// the configured data point says it is, once per connect
/// (ADR/2026-07-21-verifying-configuration-against-the-symbol-table.md). The path itself is the
/// lookup's business: an element arrives here already declared as the scalar it is.
/// </summary>
internal static class LogixTypeComparison
{
    internal static LogixTypeMismatch Compare(ResolvedDataPoint resolved)
    {
        // An absent declaration has nothing to contradict; the verifier reports that path as missing.
        if (resolved.DeclaredType is not { } declared)
        {
            return LogixTypeMismatch.None;
        }

        var dataPoint = resolved.DataPoint;

        // Rank leads: a shape that disagrees makes every comparison after it meaningless either way.
        if (declared.DimensionCount != dataPoint.DimensionCount)
        {
            return LogixTypeMismatch.Rank;
        }

        if (declared.DataType != dataPoint.DataType)
        {
            return LogixTypeMismatch.DataType;
        }

        return dataPoint switch
        {
            StringDataPoint stringDataPoint => CompareCapacity(stringDataPoint, declared),
            ILogixArrayDataPoint arrayDataPoint => CompareElementCount(arrayDataPoint, declared),
            _ => LogixTypeMismatch.None,
        };
    }

    private static LogixTypeMismatch CompareCapacity(StringDataPoint dataPoint, DeclaredType declared) =>
        declared.MaxLength == dataPoint.MaxLength
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.StringCapacity;

    private static LogixTypeMismatch CompareElementCount(ILogixArrayDataPoint dataPoint, DeclaredType declared) =>
        declared.ElementCount == dataPoint.ElementCount
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.ElementCount;
}
