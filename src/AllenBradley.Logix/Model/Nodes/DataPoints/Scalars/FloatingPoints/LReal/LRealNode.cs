using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;

/// <summary>
/// A configured <c>LREAL</c> tag, the configuration-time half of <see cref="LRealDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record LRealNode(LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency)
    : LogixTagNode(OriginalNode, TagName, PollFrequency), ILogixTagNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "LReal";

    /// <summary>
    /// <c>LREAL</c> arrived with the 5X80 controllers. Implemented explicitly, because the YAML
    /// consistency test expects every public property of a data point node to be a manifest property.
    /// </summary>
    LogixGeneration ILogixTagNode.MinimumGeneration => LogixGeneration.Logix5X80;
}
