using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.LRealArray;

/// <summary>
/// A configured one-dimensional <c>LREAL</c> array tag, the configuration-time half of
/// <see cref="LRealArrayDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
/// <param name="ElementCount">The number of elements the tag is declared with in Studio 5000.</param>
internal sealed record LRealArrayNode(
    LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency, ElementCount ElementCount)
    : LogixArrayNode(OriginalNode, TagName, PollFrequency, ElementCount), ILogixTagNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "LRealArray";

    /// <summary>
    /// An array of a type the controller has not got is not a different question from the scalar, so this
    /// carries what <c>LRealNode</c> carries. Implemented explicitly, because the YAML consistency test
    /// expects every public property of a data point node to be a manifest property.
    /// </summary>
    LogixGeneration ILogixTagNode.MinimumGeneration => LogixGeneration.Logix5X80;
}
