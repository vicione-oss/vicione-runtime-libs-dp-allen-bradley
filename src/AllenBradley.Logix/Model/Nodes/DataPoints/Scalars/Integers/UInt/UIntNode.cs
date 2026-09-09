using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UInt;

/// <summary>
/// A configured <c>UINT</c> tag. The configuration-time half of <see cref="UIntDataPoint"/>: this is
/// what the manifest produces, that is what the client reads.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record UIntNode(LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency)
    : LogixTagNode(OriginalNode, TagName, PollFrequency), ILogixTagNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "UInt";

    /// <summary>
    /// The 5X80 controllers are the ones that have the unsigned integers; a 5X70 cannot resolve a tag of
    /// this type at all.
    /// Implemented explicitly because the YAML consistency test expects every public property of a data point
    /// node to be one the manifest declares.
    /// </summary>
    LogixGeneration ILogixTagNode.MinimumGeneration => LogixGeneration.Logix5X80;
}
