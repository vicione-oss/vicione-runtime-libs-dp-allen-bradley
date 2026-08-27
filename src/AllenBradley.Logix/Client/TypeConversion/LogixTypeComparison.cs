using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// The whole type rule, in one function: the controller's TagDefinition read against what a converter
// expects the tag to be. Configuration verification is the only caller, and it runs once per connect
// (ADR-003) — reads and writes trust the verdict it reached rather than re-reaching it per operation.
//
// It is generic over the converters because the expected side is data: a kind and a data type, which is
// the same shape the controller reports the actual side in. Two converters differ in the constants they
// supply, not in how they are compared.
internal static class LogixTypeComparison
{
    // Shape is settled before type: a scalar configured against an array is a shape mismatch whatever
    // its elements hold, and comparing the two data types would be meaningless. A tag with no
    // declaration has nothing to compare, so it reports None rather than a contradiction; the verifier
    // reports that tag as absent from the controller before it ever asks here.
    internal static LogixTypeMismatch Compare(IDataPointConverter converter, TagDefinition? declaration)
    {
        if (declaration is not { } tagDefinition)
        {
            return LogixTypeMismatch.None;
        }

        if (!tagDefinition.DimensionCount.IsScalar)
        {
            return LogixTypeMismatch.Array;
        }

        if (tagDefinition.Kind != converter.ExpectedKind)
        {
            return LogixTypeMismatch.Structure;
        }

        return tagDefinition.DataType == converter.ExpectedDataType
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.AtomicType;
    }
}
