namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>A Logix <c>REAL</c> tag — an IEEE-754 single, carried as <see cref="float"/>.</summary>
/// <param name="TagName">The symbolic tag address.</param>
public sealed record RealDataPoint(TagName TagName) : ILogixDataPoint;
