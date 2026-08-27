namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// What a failed read comes home with: the point it was read for, and nothing else.
/// </summary>
/// <remarks>
/// Deliberately not an <see cref="ILogixDataPointValue{TDomain}"/>. A read that failed has no payload
/// to type, and a typed value would have to invent a <c>default</c> and hand it over as the tag's
/// contents — zero and the empty string are values a tag can genuinely hold. Quality is therefore
/// carried by which of the two shapes came back, not by a flag on a value that reads the same either
/// way.
/// </remarks>
/// <param name="DataPoint">The point whose read failed.</param>
internal sealed record BadLogixDataPointValue(ILogixDataPoint DataPoint) : ILogixDataPointValue
{
    /// <summary>Always <c>null</c>: nothing was decoded.</summary>
    public object? Value => null;

    /// <inheritdoc />
    public LogixQuality Quality => LogixQuality.Bad;

    /// <summary>
    /// Always <c>false</c>. The range gate is the outgoing port's, and only a value that converted from
    /// an engine value ever reaches it — never one of these, which the read path makes.
    /// </summary>
    public bool IsInValueRange() => false;
}
