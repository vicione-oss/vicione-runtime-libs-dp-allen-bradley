using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Floats;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>Selects the converter for each type of Legacy data point.</summary>
internal static class LegacyConverters
{
    private static readonly IntegerConverter Integer = new();
    private static readonly LongConverter Long = new();
    private static readonly FloatConverter Float = new();
    private static readonly StringConverter String = new();
    private static readonly AsciiConverter Ascii = new();
    private static readonly BitDataPointConverter Bit = new();

    /// <exception cref="InvalidOperationException">The data point has a type without a converter.</exception>
    internal static IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue> For(ILegacyDataPoint dataPoint) =>
        dataPoint switch
        {
            IntegerDataPoint => Integer,
            LongDataPoint => Long,
            FloatDataPoint => Float,
            StringDataPoint => String,
            AsciiDataPoint => Ascii,
            BitDataPoint => Bit,
            _ => throw new InvalidOperationException(
                $"No converter is known for data point type {dataPoint.GetType().Name}. Add it to LegacyConverters.For."),
        };
}
