using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

/// <summary>
/// A one-dimensional Logix <c>UDINT</c> array tag, read whole and carried as one <see cref="uint"/> array
/// of the declared length.
/// </summary>
/// <param name="TagAddress">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
/// <param name="ElementCount">The number of elements the tag is declared with in Studio 5000.</param>
public sealed record UDIntArrayDataPoint(
    TagAddress TagAddress,
    PollFrequency PollFrequency,
    Channels Channels,
    ElementCount ElementCount)
    : LogixArrayDataPoint<uint>(TagAddress, PollFrequency, Channels, ElementCount)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Udint;
}
