using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
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
/// The walk carries the scope its containers name, and composing an address is the one thing it does
/// beyond collecting: a tag under <c>ControllerTags</c> addresses itself, one under a program addresses
/// as <c>Program:MainProgram.Count</c>. The configured tag name stays bare either way — the prefix
/// belongs to the container, so a leaf never has to know which scope it hangs under.
/// </remarks>
internal sealed class LogixDataPointsGroupsMapper : IDataPointGroupsMapper<ILogixDataPoint, LogixDataPointGroup,
    DeviceNode, LogixCommunication>
{
    /// <inheritdoc />
    public IReadOnlyList<ILogixDataPoint> ToDataPoints(DeviceNode deviceNode)
    {
        return Collect(deviceNode, TagScope.Controller, []);
    }

    /// <inheritdoc />
    public LogixDataPointGroup CreateGroup(PollFrequency pollFrequency, IReadOnlyList<ILogixDataPoint> dataPoints) =>
        new(pollFrequency, dataPoints);

    private static List<ILogixDataPoint> Collect(
        IConfigurationNode configurationNode, TagScope scope, List<ILogixDataPoint> dataPoints)
    {
        dataPoints.AddRange(
            configurationNode.DataPointNodes.Select(dataPointNode => ToDataPoint(dataPointNode, scope)));

        foreach (var childNode in configurationNode.ConfigurationNodes)
        {
            Collect(childNode, ScopeOf(childNode, scope), dataPoints);
        }

        return dataPoints;
    }

    // A container that names a scope replaces the one it hangs under. Anything else passes the enclosing
    // scope through: the device root, which holds no tags of its own, and later a structure container,
    // whose path is read off the controller rather than configured.
    private static TagScope ScopeOf(IConfigurationNode configurationNode, TagScope enclosingScope) =>
        configurationNode is ILogixTagScopeNode scopeNode ? scopeNode.Scope() : enclosingScope;

    // The node-to-point pairing for each type the addon models. A node reaching here that this switch
    // does not name has a node mapper and no data point behind it, which the manifest cannot express and
    // only a half-finished type slice produces.
    private static ILogixDataPoint ToDataPoint(IDataPointNode dataPointNode, TagScope scope) => dataPointNode switch
    {
        DIntNode dInt => new DIntDataPoint(scope.Qualify(dInt.TagName), dInt.PollFrequency, dInt.Channels),
        LRealNode lReal => new LRealDataPoint(scope.Qualify(lReal.TagName), lReal.PollFrequency, lReal.Channels),
        StringNode text => new StringDataPoint(
            scope.Qualify(text.TagName), text.PollFrequency, text.Channels, text.MaxLength),
        _ => throw new NotSupportedException(
            $"Unsupported Logix data point node type '{dataPointNode.GetType().Name}'."),
    };
}
