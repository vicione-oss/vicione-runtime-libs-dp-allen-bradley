using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// Non-generic boundary the batches and registry hold, so a heterogeneous batch of data points is
// processed in one loop. Every converter derives from DataPointConverter<TDataPoint, TDomain>, which
// implements this interface and performs the single object/ILogixDataPoint boundary cast in one place.
internal interface IDataPointConverter
{
    // The CIP type this converter expects the controller to report for the tag. Checked against the
    // controller's actual type before decoding (see ConflictsWith / LogixReadBatch).
    AllenBradleyDataType ExpectedType { get; }

    // Wire size, in bytes, of one value — DINT and REAL are 4. Used to size write buffers and as the
    // decode-safety backstop when the controller's type is unknown (see LogixReadBatch).
    ByteSize ByteSize { get; }

    // How the controller's metadata disagrees with what this converter decodes — a structure, an array, or a
    // different scalar CIP type — or None when they match. This is the single comparison the run-time gate
    // and configuration verification share (ADR-003), so a mismatch means the same thing to a degraded read
    // and to a connect-time verification error. Null metadata (the tag is absent from the flat symbol table,
    // e.g. a structure member) is unverifiable, not a contradiction, so it reports None.
    LogixTypeMismatch CompareTo(TagDefinition? metadata);

    // Whether CompareTo found any disagreement — the bool the batches gate read and write on, so a tag whose
    // controller type contradicts the configuration degrades to a Bad value or a failed write instead of a
    // misread one. Null metadata does not conflict; the byte-size backstop stands in for the check there.
    bool ConflictsWith(TagDefinition? metadata);

    // Decodes the raw little-endian buffer into the data point's own typed value (Good quality). The
    // value record belongs to the data point, not to the converter: this decodes bytes to a TDomain and
    // hands it to LogixDataPoint<TDomain>.CreateLogixValue to be wrapped. A read that produced nothing
    // to wrap has no converter in it at all — see BadLogixDataPointValue.
    ILogixDataPointValue Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    // Encodes the payload carried by dataPointValue into buffer (little-endian). Rejects a value that is
    // not the typed value its data point makes — a Bad one above all, which carries no payload.
    void Encode(ILogixDataPointValue dataPointValue, Span<byte> buffer);
}
