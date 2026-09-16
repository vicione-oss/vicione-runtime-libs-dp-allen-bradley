using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.UDIntArray;

/// <summary>
/// A configured one-dimensional <c>UDINT</c> array tag, the configuration-time half of
/// <see cref="UDIntArrayDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
/// <param name="ElementCount">The number of elements the tag is declared with in Studio 5000.</param>
internal sealed record UdIntArrayDataPointNode(
    LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency, ElementCount ElementCount)
    : LogixArrayDataPointNode(OriginalNode, TagName, PollFrequency, ElementCount), ILogixDataPointNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "UDIntArray";

    public override AllenBradleyDataType DataType => AllenBradleyDataType.Udint;
}
