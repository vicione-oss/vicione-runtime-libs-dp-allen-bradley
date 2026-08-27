using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;

/// <summary>
/// Turns a configured node tree into the runtime data points both ports work from, and groups them by
/// poll frequency for the incoming one. One mapper serves both directions — the outgoing port takes the
/// flat list and ignores the grouping.
/// </summary>
/// <remarks>
/// The walk carries no address prefix, because controller scope contributes no segment to a tag address:
/// a tag configured under <c>ControllerTags</c> addresses itself. Program scope is the one that prefixes
/// (<c>Program:MainProgram.Count</c>) and will bring the accumulated path with it.
/// </remarks>
internal sealed class LogixDataPointsGroupsMapper : IDataPointGroupsMapper<ILogixDataPoint, LogixDataPointGroup,
    DeviceNode, LogixCommunication>
{
    /// <inheritdoc />
    public IReadOnlyList<ILogixDataPoint> ToDataPoints(DeviceNode deviceNode)
    {
        return Collect(deviceNode, []);
    }

    /// <inheritdoc />
    public LogixDataPointGroup CreateGroup(PollFrequency pollFrequency, IReadOnlyList<ILogixDataPoint> dataPoints) =>
        new(pollFrequency, dataPoints);

    private static List<ILogixDataPoint> Collect(IConfigurationNode configurationNode, List<ILogixDataPoint> dataPoints)
    {
        dataPoints.AddRange(configurationNode.DataPointNodes.Select(ToDataPoint));

        foreach (var childNode in configurationNode.ConfigurationNodes)
        {
            dataPoints.AddRange(Collect(childNode, []));
        }

        return dataPoints;
    }

    // The node-to-point pairing for each type the addon models. A node reaching here that this switch
    // does not name has a node mapper and no data point behind it, which the manifest cannot express and
    // only a half-finished type slice produces.
    private static ILogixDataPoint ToDataPoint(IDataPointNode dataPointNode) => dataPointNode switch
    {
        DIntNode dInt => new DIntDataPoint(dInt.TagName, dInt.PollFrequency, dInt.Channels),
        StringNode text => new StringDataPoint(text.TagName, text.PollFrequency, text.Channels, text.MaxLength),
        _ => throw new NotSupportedException(
            $"Unsupported Logix data point node type '{dataPointNode.GetType().Name}'."),
    };
}
