using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.LRealArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.FloatingPoints.RealArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.DIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.IntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.LIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.SIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.UDIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.UIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.ULIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Integers.USIntArray;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Booleans.Bool;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.LReal;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.FloatingPoints.Real;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.DInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.Int;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.LInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.SInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UDInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.UInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.ULInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Integers.USInt;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.Mapping;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;

/// <summary>
/// Turns a configured node tree into the runtime data points both ports work from, grouped by poll
/// frequency for the incoming one. The outgoing port takes the flat list and ignores the grouping.
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
        var logixDataPoints = configurationNode.DataPointNodes
            .OfType<ILogixTagNode>()
            .Select(dataPointNode => ToDataPoint(dataPointNode, scope));

        dataPoints.AddRange(logixDataPoints);

        foreach (var childNode in configurationNode.ConfigurationNodes)
        {
            Collect(childNode, ScopeOf(childNode, scope), dataPoints);
        }

        return dataPoints;
    }

    private static TagScope ScopeOf(IConfigurationNode configurationNode, TagScope enclosingScope) =>
        configurationNode is ITagScopeNode scopeNode ? scopeNode.Scope() : enclosingScope;

    private static ILogixDataPoint ToDataPoint(ILogixTagNode dataPointNode, TagScope scope)
    {
        var pollFrequency = dataPointNode.PollFrequency;
        var channels = dataPointNode.Channels;
        var tagName = scope.Qualify(dataPointNode.TagName);

        return dataPointNode switch
        {
            BoolNode => new BoolDataPoint(tagName, pollFrequency, channels),
            SIntNode => new SIntDataPoint(tagName, pollFrequency, channels),
            IntNode => new IntDataPoint(tagName, pollFrequency, channels),
            DIntNode => new DIntDataPoint(tagName, pollFrequency, channels),
            LIntNode => new LIntDataPoint(tagName, pollFrequency, channels),
            USIntNode => new USIntDataPoint(tagName, pollFrequency, channels),
            UIntNode => new UIntDataPoint(tagName, pollFrequency, channels),
            UDIntNode => new UDIntDataPoint(tagName, pollFrequency, channels),
            ULIntNode => new ULIntDataPoint(tagName, pollFrequency, channels),
            RealNode => new RealDataPoint(tagName, pollFrequency, channels),
            LRealNode => new LRealDataPoint(tagName, pollFrequency, channels),
            StringNode stringNode => new StringDataPoint(tagName, pollFrequency, channels, stringNode.MaxLength),
            LogixArrayNode arrayNode => ToArrayDataPoint(arrayNode, tagName),
            _ => throw UnsupportedNode(dataPointNode),
        };
    }

    private static ILogixDataPoint ToArrayDataPoint(LogixArrayNode arrayNode, TagName tagName)
    {
        var pollFrequency = arrayNode.PollFrequency;
        var channels = arrayNode.Channels;
        var elementCount = arrayNode.ElementCount;

        return arrayNode switch
        {
            SIntArrayNode => new SIntArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            IntArrayNode => new IntArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            DIntArrayNode => new DIntArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            LIntArrayNode => new LIntArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            USIntArrayNode => new USIntArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            UIntArrayNode => new UIntArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            UDIntArrayNode => new UDIntArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            ULIntArrayNode => new ULIntArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            RealArrayNode => new RealArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            LRealArrayNode => new LRealArrayDataPoint(tagName, pollFrequency, channels, elementCount),
            _ => throw UnsupportedNode(arrayNode),
        };
    }

    // Unreachable from a manifest: a node the switches do not name has a node mapper and no data
    // point behind it, which only a half-finished type slice produces.
    private static NotSupportedException UnsupportedNode(ILogixTagNode dataPointNode) =>
        new($"Unsupported Logix data point node type '{dataPointNode.GetType().Name}'.");
}
