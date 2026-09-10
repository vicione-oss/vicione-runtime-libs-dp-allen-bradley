using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// The result of reading, or the payload for writing, one data point: the originating point and the
/// value it carries. A read that produced nothing leaves its point out of the batch's values
/// (<c>ADR/2026-07-16-reading-and-writing-a-group-of-tags.md</c>), so a value in hand is one that was read.
/// </summary>
public interface ILogixDataPointValue : IDataPointValue
{
    new ILogixDataPoint DataPoint { get; }

    /// <summary>
    /// Narrows the framework's view of the originating point to ours, so an implementation declares its
    /// data point once.
    /// </summary>
    IDataPoint IDataPointValue.DataPoint => DataPoint;
}

/// <summary>
/// A value whose payload is typed to <typeparamref name="TDomain"/>, the .NET type its data point
/// exchanges. Only <see cref="LogixDataPoint{TDomain}"/> makes one, and only its own, so a payload cannot
/// disagree with its point about its type.
/// </summary>
/// <typeparam name="TDomain">The .NET type the data point exchanges — <c>int</c> for a <c>DINT</c>.</typeparam>
internal interface ILogixDataPointValue<out TDomain> : ILogixDataPointValue, ITypedDataPointValue<TDomain>
{
    /// <inheritdoc />
    object? IDataPointValue.Value => TypedValue;
}
