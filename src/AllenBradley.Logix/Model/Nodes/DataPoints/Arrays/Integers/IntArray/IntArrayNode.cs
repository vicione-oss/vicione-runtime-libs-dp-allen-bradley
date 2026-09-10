using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray;

/// <summary>
/// A configured one-dimensional <c>INT</c> array tag, the configuration-time half of
/// <see cref="IntArrayDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
/// <param name="ElementCount">The number of elements the tag is declared with in Studio 5000.</param>
internal sealed record IntArrayNode(
    LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency, ElementCount ElementCount)
    : LogixArrayNode(OriginalNode, TagName, PollFrequency, ElementCount)
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "IntArray";
}
