namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;

/// <summary>
/// Resolves the tag a data point is read and written through. A returned tag is borrowed, never owned:
/// the source disposes it.
/// </summary>
/// <typeparam name="TDataPoint">The dataport's data point type.</typeparam>
/// <typeparam name="TTag">The tag the dataport hands out; the batches only need it to be an <see cref="ITagAccess"/>.</typeparam>
internal interface ITagSource<in TDataPoint, out TTag>
    where TTag : ITagAccess
{
    /// <summary>Returns the tag for <paramref name="dataPoint"/>, creating or reusing it.</summary>
    TTag TagFor(TDataPoint dataPoint);
}
