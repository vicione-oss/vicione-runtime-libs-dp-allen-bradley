using System.Collections.Frozen;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// Single source of truth for how each data-point type maps to a converter. Keyed by the data point's
// concrete .NET type; exhaustive by design — a lookup miss is a data point added without a converter
// and throws with a pointer here. The generic Register helper keys each converter under its own
// TDataPoint, so a converter cannot be wired to a data point type it does not handle (compile error).
internal static class DataPointConverterRegistry
{
    private static readonly FrozenDictionary<Type, IDataPointConverter> Converters = Build();

    internal static IDataPointConverter GetConverter(ILogixDataPoint dataPoint) =>
        Converters.TryGetValue(dataPoint.GetType(), out var converter)
            ? converter
            : throw new InvalidOperationException(
                $"No converter is registered for data point type {dataPoint.GetType().Name}. " +
                "Add a Register call in DataPointConverterRegistry.Build.");

    private static FrozenDictionary<Type, IDataPointConverter> Build()
    {
        var converters = new Dictionary<Type, IDataPointConverter>();
        Register(converters, new DIntConverter());
        Register(converters, new RealConverter());
        return converters.ToFrozenDictionary();
    }

    // The key is the converter's own TDataPoint, so registration cannot map a converter to a data
    // point type it does not handle.
    private static void Register<TDataPoint, TDomain>(
        Dictionary<Type, IDataPointConverter> converters,
        DataPointConverter<TDataPoint, TDomain> converter)
        where TDataPoint : LogixDataPoint<TDomain> =>
        converters.Add(typeof(TDataPoint), converter);
}
