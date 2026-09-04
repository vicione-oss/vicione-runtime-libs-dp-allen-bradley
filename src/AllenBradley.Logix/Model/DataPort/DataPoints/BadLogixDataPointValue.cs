namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// What a failed read comes home with: the point it was read for, and nothing else.
/// Quality is carried by which shape comes back, so no <c>default</c> payload is ever invented for a tag that
/// could genuinely hold zero or the empty string.
/// </summary>
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
