using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Booleans.Bool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.LInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.SInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.USInt;
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
/// The walk composes each tag's address from the scope its container names, so a configured tag name stays
/// bare.
/// </summary>
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
        configurationNode is ITagScopeNode scopeNode ? scopeNode.Scope() : enclosingScope;

    // The node-to-point pairing for each type the addon models. A node reaching here that this switch
    // does not name has a node mapper and no data point behind it, which the manifest cannot express and
    // only a half-finished type slice produces.
    private static ILogixDataPoint ToDataPoint(IDataPointNode dataPointNode, TagScope scope) => dataPointNode switch
    {
        DIntNode dInt => new DIntDataPoint(scope.Qualify(dInt.TagName), dInt.PollFrequency, dInt.Channels),
        SIntNode sInt => new SIntDataPoint(scope.Qualify(sInt.TagName), sInt.PollFrequency, sInt.Channels),
        LIntNode lInt => new LIntDataPoint(scope.Qualify(lInt.TagName), lInt.PollFrequency, lInt.Channels),
        USIntNode usInt => new USIntDataPoint(scope.Qualify(usInt.TagName), usInt.PollFrequency, usInt.Channels),
        BoolNode flag => new BoolDataPoint(scope.Qualify(flag.TagName), flag.PollFrequency, flag.Channels),
        IntNode integer => new IntDataPoint(
            scope.Qualify(integer.TagName), integer.PollFrequency, integer.Channels),
        LRealNode lReal => new LRealDataPoint(scope.Qualify(lReal.TagName), lReal.PollFrequency, lReal.Channels),
        StringNode text => new StringDataPoint(
            scope.Qualify(text.TagName), text.PollFrequency, text.Channels, text.MaxLength),
        _ => throw new NotSupportedException(
            $"Unsupported Logix data point node type '{dataPointNode.GetType().Name}'."),
    };
}
