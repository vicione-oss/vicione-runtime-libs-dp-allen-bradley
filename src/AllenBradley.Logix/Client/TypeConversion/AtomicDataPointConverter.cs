using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// Base for the elementary CIP types — the ones the controller reports by a one-byte type code and that
// occupy a fixed number of bytes whatever they are configured as. It supplies the one thing that is the
// same for every one of them: nothing about their size is configurable, so nothing about their size is
// theirs to state.
//
// A subclass names its data type and nothing else about matching is its concern — the tag it will
// accept follows from that and the kind below, and LogixTypeComparison reads them.
internal abstract class AtomicDataPointConverter<TDataPoint, TDomain> : DataPointConverter<TDataPoint, TDomain>
    where TDataPoint : LogixDataPoint<TDomain>
{
    public sealed override LogixTypeKind ExpectedKind => LogixTypeKind.Atomic;

    // Nothing to agree on: the size an elementary type occupies follows from the type, so a capacity is
    // not part of what these converters expect.
    protected sealed override StringMaxLength? MaxLengthOf(TDataPoint dataPoint) => null;
}
