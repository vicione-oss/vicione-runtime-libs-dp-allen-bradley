using System.Collections.Frozen;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// Single source of truth for how each data-point type maps to a converter, keyed by the data point's
/// concrete .NET type. Exhaustive by design: a lookup miss is a data point added without a converter.
/// </summary>
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
        Register(converters, new ULIntConverter());
        Register(converters, new RealConverter());
        Register(converters, new LRealConverter());
        Register(converters, new LogixStringConverter());
        Register(converters, new SIntArrayConverter());
        Register(converters, new IntArrayConverter());
        Register(converters, new DIntArrayConverter());
        Register(converters, new LIntArrayConverter());

        return converters.ToFrozenDictionary();
    }

    // Infers the key from the converter's own type parameter rather than taking it, so filing a converter
    // under a data point it does not handle is a compile error rather than a run-time routing failure.
    private static void Register<TDataPoint, TDomain>(
        Dictionary<Type, IDataPointConverter> converters,
        DataPointConverter<TDataPoint, TDomain> converter)
        where TDataPoint : LogixDataPoint<TDomain> =>
        converters.Add(typeof(TDataPoint), converter);
}
