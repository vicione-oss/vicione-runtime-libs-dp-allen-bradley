using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// Non-generic boundary the batches and registry hold, so a heterogeneous batch of data points is
// processed in one loop. Every converter derives from DataPointConverter<TDataPoint, TDomain>, which
// implements this interface and performs the single object/ILogixDataPoint boundary cast in one place.
internal interface IDataPointConverter
{
    // What this converter expects the tag to be, in the spelling Studio 5000 uses — DINT, REAL, STRING.
    // Display, never identity: it names the type in a verification message and in a rejected write, and
    // nothing is decided by it. The comparison is made from the two members below.
    LogixDataTypeName ExpectedTypeName { get; }

    // The expected side of the type comparison, in the same fields the controller's own TagDefinition
    // reports the actual side in — a kind, a data type, and a capacity. Stating the expectation as data
    // rather than as a comparison of its own is what lets one function (LogixTypeComparison) hold the
    // whole rule: what a converter decodes says what the tag must be, and saying whether it is says the
    // same thing for every converter.
    LogixTypeKind ExpectedKind { get; }

    // The data type this converter expects the controller to declare — Dint, Real, String.
    AllenBradleyDataType? ExpectedDataType { get; }

    // The character capacity this converter expects the controller to declare, or null for a type whose
    // size the type alone fixes. It takes the data point because a STRING_20 and a STRING are one
    // converter and two capacities, so the expectation is configuration rather than a constant of the
    // converter.
    StringMaxLength? MaxLengthFor(ILogixDataPoint dataPoint);

    // Decodes the raw little-endian buffer into the data point's own typed value (Good quality). The
    // value record belongs to the data point, not to the converter: this decodes bytes to a TDomain and
    // hands it to LogixDataPoint<TDomain>.CreateLogixValue to be wrapped. A read that produced nothing
    // to wrap has no converter in it at all — see BadLogixDataPointValue.
    ILogixDataPointValue Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    // Encodes the payload carried by dataPointValue into the bytes it occupies on the wire
    // (little-endian): the type's fixed width for an elementary type, .LEN plus the configured .DATA for
    // a STRING. This is what the value is, not how wide the tag is — the controller pads a structure to
    // its own boundary, and only libplctag's handle knows that width; it takes these bytes as they are
    // and refuses any longer than itself. Rejects a value that is not the typed value its data point
    // makes — a Bad one above all, which carries no payload.
    byte[] Encode(ILogixDataPointValue dataPointValue);
}
