using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// The type of a data file, which is the type of every element in it. A closed set of values, one static
/// member each.
/// </summary>
/// <param name="Letter">The letter an address starts with — the <c>N</c> in <c>N7:0</c>.</param>
/// <param name="Name">The type as RSLogix 500 names it in its list of data files.</param>
public readonly record struct DataFileType(string Letter, DataTypeName Name)
{
    /// <summary><c>N</c> — 16-bit signed integers, one word per element.</summary>
    public static readonly DataFileType Integer = new("N", new DataTypeName("Integer"));
}
