using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.USInt;

/// <summary>
/// A configured <c>USINT</c> tag. The configuration-time half of <see cref="USIntDataPoint"/>: this is
/// what the manifest produces, that is what the client reads.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record USIntNode(LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency)
    : LogixScalarNode(OriginalNode, TagName, PollFrequency), ILogixScalarNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "USInt";

    /// <summary>
    /// The 5X80 controllers are the ones that have the unsigned integers; a 5X70 cannot resolve a tag of
    /// this type at all.
    /// Implemented explicitly because the YAML consistency test expects every public property of a data point
    /// node to be one the manifest declares.
    /// </summary>
    LogixGeneration ILogixScalarNode.MinimumGeneration => LogixGeneration.Logix5X80;
}
