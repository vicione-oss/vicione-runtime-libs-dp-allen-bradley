namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;

/// <summary>
/// Turns a tag's raw bytes into a data point's typed value and back. A dataport implements it once per
/// data point type and hands the batches the boundary they hold, so one loop processes a batch of mixed
/// data point types (ADR/2026-07-16-decoding-tag-bytes-into-typed-values.md).
/// </summary>
internal interface IDataPointConverter<in TDataPoint, TValue>
{
    /// <summary>Decodes the raw little-endian buffer into the data point's own typed value.</summary>
    /// <exception cref="DecodeException">The buffer does not fit the shape the data point was configured as.</exception>
    TValue Decode(TDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Encodes the payload carried by <paramref name="value"/> into the little-endian bytes the value
    /// occupies. A structure includes its trailing pad, because a controller refuses a write shorter than the
    /// whole structure.
    /// </summary>
    /// <exception cref="InvalidOperationException">The value cannot be written to its data point.</exception>
    byte[] Encode(TValue value);
}
