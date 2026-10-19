using System.Globalization;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// Where a data point's value lives: the type and number of its data file, the element inside it, and the
/// sub-element or the bit inside the element where the value is only a part of it. The tree walk fills the parts, and the text a
/// controller is asked for is rendered from them.
/// </summary>
/// <param name="FileType">The type of the data file, which fixes the type of the value.</param>
/// <param name="FileNumber">The number of the data file.</param>
/// <param name="ElementNumber">The element inside the data file.</param>
/// <param name="BitNumber">The bit inside the element, or <c>null</c> when the value is the whole element.</param>
/// <param name="SubElement">The word inside the element, or <c>null</c> when the value is the whole element.</param>
public readonly record struct DataFileAddress(
    DataFileType FileType,
    FileNumber FileNumber,
    ElementNumber ElementNumber,
    BitNumber? BitNumber = null,
    SubElement? SubElement = null)
{
    /// <summary>
    /// The address as RSLogix 500 writes it — <c>N7:12</c>, <c>B3:2/5</c> for a bit, or <c>T4:0.ACC</c> for a
    /// sub-element.
    /// </summary>
    public override string ToString() =>
        (BitNumber, SubElement) switch
        {
            ({ } bitNumber, _) => string.Create(CultureInfo.InvariantCulture, $"{ElementAddress()}/{bitNumber.Value}"),
            (_, { } subElement) => $"{ElementAddress()}.{subElement.Mnemonic}",
            _ => ElementAddress(),
        };

    private string ElementAddress() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{FileType.Letter}{FileNumber.Value}:{ElementNumber.Value}");
}
