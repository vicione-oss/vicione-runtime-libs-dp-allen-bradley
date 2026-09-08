using System.Collections.Frozen;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;
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

        Register(converters, new BoolConverter());
        Register(converters, new SIntConverter());
        Register(converters, new IntConverter());
        Register(converters, new DIntConverter());
        Register(converters, new LIntConverter());
        Register(converters, new USIntConverter());
        Register(converters, new UIntConverter());
        Register(converters, new UDIntConverter());
        Register(converters, new RealConverter());
        Register(converters, new LRealConverter());
        Register(converters, new LogixStringConverter());

        return converters.ToFrozenDictionary();
    }

    // Derives the key from the converter instead of taking one, which is what makes a mis-registration
    // unrepresentable rather than merely unlikely: TDataPoint is inferred from the converter's own base
    // (DIntConverter is an AtomicDataPointConverter<DIntDataPoint, int>), so there is no second place
    // for a key to disagree with the converter filed under it. A literal dictionary entry would take
    // both halves from the caller and compile happily with them mismatched, failing only at run time in
    // DataPointConverter.Cast.
    private static void Register<TDataPoint, TDomain>(
        Dictionary<Type, IDataPointConverter> converters,
        DataPointConverter<TDataPoint, TDomain> converter)
        where TDataPoint : LogixDataPoint<TDomain> =>
        converters.Add(typeof(TDataPoint), converter);
}
