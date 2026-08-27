namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>A Logix <c>DINT</c> tag — a 32-bit signed integer, carried as <see cref="int"/>.</summary>
/// <param name="TagName">The symbolic tag address.</param>
public sealed record DIntDataPoint(TagName TagName) : ILogixDataPoint;
