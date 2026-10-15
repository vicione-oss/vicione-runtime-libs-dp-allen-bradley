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

    /// <summary><c>L</c> — 32-bit signed integers, two words per element.</summary>
    public static readonly DataFileType Long = new("L", new DataTypeName("Long"));

    /// <summary><c>F</c> — 32-bit IEEE 754 floating-point values, two words per element.</summary>
    public static readonly DataFileType Float = new("F", new DataTypeName("Float"));

    /// <summary><c>B</c> — 16 bits per element, each addressed on its own.</summary>
    public static readonly DataFileType Binary = new("B", new DataTypeName("Binary"));

    /// <summary><c>S</c> — the status file of the controller, always file 2, one word per element.</summary>
    public static readonly DataFileType Status = new("S", new DataTypeName("Status"));
}
