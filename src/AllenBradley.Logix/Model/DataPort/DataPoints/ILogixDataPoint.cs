using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// A single Logix tag reference: the symbolic address of a tag to read or write. Concrete shapes
/// (<c>DIntDataPoint</c>, <c>RealDataPoint</c>, …) name the point's type by identity alone; the CIP
/// type it expects and the .NET value type it carries are both bound by its converter, the single source
/// of that mapping.
/// </summary>
/// <remarks>
/// An <see cref="IPollingDataPoint"/>, so the incoming port can group points by how often they are
/// polled and the framework can name one in a log. The members that serve those two purposes —
/// <see cref="IDataPoint.Identifier"/>, <see cref="IDataPoint.DataTypeName"/>,
/// <see cref="IDataPoint.Channels"/> and <see cref="IPollingDataPoint.PollFrequency"/> — are carried by
/// <see cref="LogixDataPoint{TDomain}"/>, which every concrete shape derives from.
/// </remarks>
public interface ILogixDataPoint : IPollingDataPoint
{
    /// <summary>The symbolic tag address, e.g. <c>Motor.Speed</c> or <c>Program:Main.Count</c>.</summary>
    TagName TagName { get; }

    /// <summary>
    /// Turns an untyped engine value into one of this point's typed values, or says why it will not
    /// convert. The outgoing port's first gate, and the only door an <c>object?</c> enters the model
    /// through: past it, a value's payload is the point's own .NET type by construction.
    /// </summary>
    /// <param name="value">The engine value to write.</param>
    /// <returns>
    /// A <c>ConvertedDataPointValue</c> holding the typed value, or a <c>NotConvertedDataPointValue</c>
    /// naming the point and the type that was wanted. A failure is returned rather than thrown so the
    /// framework can log it and drop the batch.
    /// </returns>
    DataPointValueConversion<ILogixDataPointValue> ConvertValue(object? value);
}
