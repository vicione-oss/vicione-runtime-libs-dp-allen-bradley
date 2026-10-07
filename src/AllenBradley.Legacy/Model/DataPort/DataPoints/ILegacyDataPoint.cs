using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// One element of a data file to read or write. Concrete shapes (<c>IntegerDataPoint</c>, …) name the
/// point's type by identity alone.
/// </summary>
public interface ILegacyDataPoint : IPollingDataPoint
{
    /// <summary>Where the value lives: the data file and the element inside it.</summary>
    DataFileAddress Address { get; }

    /// <summary>
    /// Turns an untyped engine value into one of this point's typed values, or says why it will not
    /// convert. The only door an <c>object?</c> enters the model through.
    /// </summary>
    /// <returns>
    /// A <c>ConvertedDataPointValue</c> holding the typed value, or a <c>NotConvertedDataPointValue</c>
    /// naming the point and the type that was wanted — returned rather than thrown, so the framework can
    /// log it and drop the batch.
    /// </returns>
    DataPointValueConversion<ILegacyDataPointValue> ConvertValue(object? value);
}
