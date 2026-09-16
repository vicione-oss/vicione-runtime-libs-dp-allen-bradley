using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray;

/// <summary>
/// A configured one-dimensional <c>BOOL</c> array tag, the configuration-time half of
/// <see cref="BoolArrayDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
/// <param name="ElementCount">The number of bits the tag is declared with in Studio 5000.</param>
internal sealed record BoolArrayDataPointNode(
    LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency, ElementCount ElementCount)
    : LogixArrayDataPointNode(OriginalNode, TagName, PollFrequency, ElementCount)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "BoolArray";

    public override AllenBradleyDataType DataType => AllenBradleyDataType.Bool;
}
