namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// A single Logix tag reference: the symbolic address of a tag to read or write. Concrete shapes
/// (<c>DIntDataPoint</c>, <c>RealDataPoint</c>, …) name the point's type by identity alone; the CIP
/// type it expects and the .NET value type it carries are both bound by its converter, the single source
/// of that mapping.
/// </summary>
public interface ILogixDataPoint
{
    /// <summary>The symbolic tag address, e.g. <c>Motor.Speed</c> or <c>Program:Main.Count</c>.</summary>
    TagName TagName { get; }
}
