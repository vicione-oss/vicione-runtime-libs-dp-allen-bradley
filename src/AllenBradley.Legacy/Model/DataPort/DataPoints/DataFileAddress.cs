using System.Globalization;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// Where a data point's value lives: the type and number of its data file, the element inside it, and the
/// bit inside the element where the value is a single bit. The tree walk fills the parts, and the text a
/// controller is asked for is rendered from them.
/// </summary>
/// <param name="FileType">The type of the data file, which fixes the type of the value.</param>
/// <param name="FileNumber">The number of the data file.</param>
/// <param name="ElementNumber">The element inside the data file.</param>
/// <param name="BitNumber">The bit inside the element, or <c>null</c> when the value is the whole element.</param>
public readonly record struct DataFileAddress(
    DataFileType FileType,
    FileNumber FileNumber,
    ElementNumber ElementNumber,
    BitNumber? BitNumber = null)
{
    /// <summary>The address as RSLogix 500 writes it — <c>N7:12</c>, or <c>B3:2/5</c> for a bit.</summary>
    public override string ToString() =>
        BitNumber is { } bitNumber
            ? string.Create(CultureInfo.InvariantCulture, $"{ElementAddress()}/{bitNumber.Value}")
            : ElementAddress();

    private string ElementAddress() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{FileType.Letter}{FileNumber.Value}:{ElementNumber.Value}");
}
