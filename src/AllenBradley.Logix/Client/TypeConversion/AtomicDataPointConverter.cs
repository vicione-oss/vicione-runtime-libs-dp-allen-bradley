using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// Base for the elementary CIP types — the ones the controller reports by a one-byte type code — whether
/// the tag holds one of them or an array of them. A subclass names its data type; the rest of the
/// expectation follows from the kind fixed here.
/// </summary>
internal abstract class AtomicDataPointConverter<TDataPoint, TDomain> : DataPointConverter<TDataPoint, TDomain>
    where TDataPoint : LogixDataPoint<TDomain>
{
    public sealed override LogixTypeKind ExpectedKind => LogixTypeKind.Atomic;

    // A capacity is the n of a STRING's .DATA, which no elementary type has.
    protected sealed override StringMaxLength? MaxLengthOf(TDataPoint dataPoint) => null;
}
