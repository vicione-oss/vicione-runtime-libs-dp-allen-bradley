namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// The result of reading (or the payload for writing) one data point: the originating point, the
/// value, and its quality. <see cref="LogixDataPointValue{TDomain}"/> is the typed implementation.
/// </summary>
public interface ILogixDataPointValue
{
    /// <summary>The data point this value belongs to.</summary>
    ILogixDataPoint DataPoint { get; }

    /// <summary>The value, boxed. Not meaningful when <see cref="Quality"/> is <see cref="LogixQuality.Bad"/>.</summary>
    object? Value { get; }

    /// <summary>Whether the value is trustworthy.</summary>
    LogixQuality Quality { get; }
}
