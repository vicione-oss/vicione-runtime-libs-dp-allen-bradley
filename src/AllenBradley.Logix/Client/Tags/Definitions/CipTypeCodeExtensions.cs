using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// Translates a wire type code into the model's <see cref="AllenBradleyDataType"/>. This is the one
/// place the two vocabularies meet, so the codes stop at the decoder.
/// </summary>
internal static class CipTypeCodeExtensions
{
    /// <summary>
    /// The data type a code names, or <see cref="AllenBradleyDataType.Unknown"/> for a code outside the
    /// elementary range — a controller reporting one is describing a tag this addon cannot decode, and
    /// saying so beats passing the raw byte upwards.
    /// </summary>
    public static AllenBradleyDataType ToDataType(this CipTypeCode code) => code switch
    {
        CipTypeCode.Bool => AllenBradleyDataType.Bool,
        CipTypeCode.Sint => AllenBradleyDataType.Sint,
        CipTypeCode.Int => AllenBradleyDataType.Int,
        CipTypeCode.Dint => AllenBradleyDataType.Dint,
        CipTypeCode.Lint => AllenBradleyDataType.Lint,
        CipTypeCode.Usint => AllenBradleyDataType.Usint,
        CipTypeCode.Uint => AllenBradleyDataType.Uint,
        CipTypeCode.Udint => AllenBradleyDataType.Udint,
        CipTypeCode.Ulint => AllenBradleyDataType.Ulint,
        CipTypeCode.Real => AllenBradleyDataType.Real,
        CipTypeCode.Lreal => AllenBradleyDataType.Lreal,
        _ => AllenBradleyDataType.Unknown,
    };
}
