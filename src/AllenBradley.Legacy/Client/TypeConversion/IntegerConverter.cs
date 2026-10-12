using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>Converts an element of an integer (<c>N</c>) file, one 16-bit word, to a <see cref="short"/> and back.</summary>
internal sealed class IntegerConverter : IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue>
{
    private readonly Int16Codec _codec = new();

    public ILegacyDataPointValue Decode(ILegacyDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        Cast(dataPoint).CreateLegacyValue(_codec.Decode(buffer));

    public byte[] Encode(ILegacyDataPointValue value)
    {
        var dataPoint = Cast(value.DataPoint);

        // Only a data point creates its values, so a value of another type can only come from outside the dataport.
        if (value is not ILegacyDataPointValue<short> integer)
        {
            throw new InvalidOperationException(
                $"Cannot write {value.GetType().Name} to {dataPoint.Address}; expected a value that carries a short.");
        }

        return _codec.Encode(integer.TypedValue);
    }

    private static IntegerDataPoint Cast(ILegacyDataPoint dataPoint) =>
        dataPoint as IntegerDataPoint ?? throw new InvalidOperationException(
            $"The integer converter received {dataPoint.GetType().Name}; LegacyConverters routed the wrong converter.");
}
