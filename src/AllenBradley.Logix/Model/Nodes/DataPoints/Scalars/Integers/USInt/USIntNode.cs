using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.USInt;

/// <summary>
/// A configured <c>USINT</c> tag, the configuration-time half of <see cref="USIntDataPoint"/>.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads it.</param>
internal sealed record USIntNode(LinkedNode OriginalNode, TagName TagName, PollFrequency PollFrequency)
    : LogixDataPointNode(OriginalNode, TagName, PollFrequency), ILogixDataPointNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "USInt";

    /// <summary>
    /// The unsigned integers arrived with the 5X80 controllers. Implemented explicitly, because the YAML
    /// consistency test expects every public property of a data point node to be a manifest property.
    /// </summary>
    LogixGeneration ILogixDataPointNode.MinimumGeneration => LogixGeneration.Logix5X80;
}
