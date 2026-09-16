using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// The non-generic boundary the batches and the registry hold, so one loop processes a batch of mixed data
/// point types. The expectations stated here are what LogixTypeComparison reads at connect
/// (ADR/2026-07-16-decoding-tag-bytes-into-typed-values.md).
/// </summary>
internal interface IDataPointConverter
{
    LogixTypeKind ExpectedKind { get; }

    /// <summary>The type this converter decodes; for an array converter, the element type.</summary>
    AllenBradleyDataType ExpectedDataType { get; }

    DimensionCount ExpectedDimensionCount { get; }

    /// <summary>
    /// The expected type in the spelling Studio 5000 uses — DINT, REAL[], STRING. Display, never identity:
    /// it names the type in a verification message, and nothing is decided by it.
    /// </summary>
    string ExpectedTypeName =>
        ExpectedDimensionCount.IsScalar ? ExpectedDataType.Name : $"{ExpectedDataType.Name}[]";

    /// <summary>
    /// The character capacity the controller must declare for <paramref name="dataPoint"/>, or null for a
    /// type whose size the type alone fixes. A STRING and a STRING_20 are one converter and two capacities.
    /// </summary>
    StringMaxLength? MaxLengthFor(ILogixDataPoint dataPoint);

    /// <summary>
    /// How many elements the controller must declare for <paramref name="dataPoint"/>, or null for a shape
    /// with no extent to configure. An INT[10] and an INT[20] are one converter and two counts.
    /// </summary>
    ElementCount? ElementCountFor(ILogixDataPoint dataPoint);

    /// <summary>Decodes the raw little-endian buffer into the data point's own typed value.</summary>
    ILogixDataPointValue Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Encodes the payload carried by <paramref name="dataPointValue"/> into the little-endian bytes the
    /// value occupies — never the padded width of the tag, which only libplctag's handle knows. Rejects a
    /// value that is not the typed value its data point makes.
    /// </summary>
    byte[] Encode(ILogixDataPointValue dataPointValue);
}
