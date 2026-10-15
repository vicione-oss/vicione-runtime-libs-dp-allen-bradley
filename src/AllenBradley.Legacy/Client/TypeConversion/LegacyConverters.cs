using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>Selects the converter for each type of Legacy data point.</summary>
internal static class LegacyConverters
{
    private static readonly IntegerConverter Integer = new();
    private static readonly BitDataPointConverter Bit = new();

    /// <exception cref="InvalidOperationException">The data point has a type without a converter.</exception>
    internal static IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue> For(ILegacyDataPoint dataPoint) =>
        dataPoint switch
        {
            IntegerDataPoint => Integer,
            BitDataPoint => Bit,
            _ => throw new InvalidOperationException(
                $"No converter is known for data point type {dataPoint.GetType().Name}. Add it to LegacyConverters.For."),
        };
}
