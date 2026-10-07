using System.Globalization;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// Where a data point's value lives: the type and number of its data file, and the element inside it. The
/// tree walk fills the parts, and the text a controller is asked for is rendered from them.
/// </summary>
/// <param name="FileType">The type of the data file, which fixes the type of the value.</param>
/// <param name="FileNumber">The number of the data file.</param>
/// <param name="ElementNumber">The element inside the data file.</param>
public readonly record struct DataFileAddress(DataFileType FileType, FileNumber FileNumber, ElementNumber ElementNumber)
{
    /// <summary>The address as RSLogix 500 writes it — <c>N7:12</c>.</summary>
    public override string ToString() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{FileType.Letter}{FileNumber.Value}:{ElementNumber.Value}");
}
