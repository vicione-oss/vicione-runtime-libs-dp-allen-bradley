using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// Base for the elementary CIP types — the ones the controller reports by a one-byte type code, whether
// the tag holds one of them or an array of them. It supplies the one thing that is the same for every one
// of them: a capacity is a STRING's, so none of these has one to agree on.
//
// A subclass names its data type and nothing else about matching is its concern — the tag it will
// accept follows from that and the kind below, and LogixTypeComparison reads them.
internal abstract class AtomicDataPointConverter<TDataPoint, TDomain> : DataPointConverter<TDataPoint, TDomain>
    where TDataPoint : LogixDataPoint<TDomain>
{
    public sealed override LogixTypeKind ExpectedKind => LogixTypeKind.Atomic;

    // Nothing to agree on: a capacity is the n of a STRING's .DATA, which no elementary type has — an
    // array of one is sized by its element count instead, and that is stated beside this.
    protected sealed override StringMaxLength? MaxLengthOf(TDataPoint dataPoint) => null;
}
