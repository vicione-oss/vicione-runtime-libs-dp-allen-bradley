using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// The non-generic boundary the batches and the registry hold, so one loop processes a batch of mixed data
/// point types (ADR/2026-07-16-decoding-tag-bytes-into-typed-values.md).
/// </summary>
internal interface IDataPointConverter
{
    /// <summary>Decodes the raw little-endian buffer into the data point's own typed value.</summary>
    ILogixDataPointValue Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Encodes the payload carried by <paramref name="dataPointValue"/> into the little-endian bytes the
    /// value occupies — never the padded width of the tag, which only libplctag's handle knows. Rejects a
    /// value that is not the typed value its data point makes.
    /// </summary>
    byte[] Encode(ILogixDataPointValue dataPointValue);
}
