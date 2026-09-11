using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
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
        dataPoints.AddRange(
            configurationNode.DataPointNodes.Select(dataPointNode => ToDataPoint(dataPointNode, scope)));

        foreach (var childNode in configurationNode.ConfigurationNodes)
        {
            Collect(childNode, ScopeOf(childNode, scope), dataPoints);
        }

        return dataPoints;
    }

    private static TagScope ScopeOf(IConfigurationNode configurationNode, TagScope enclosingScope) =>
        configurationNode is ITagScopeNode scopeNode ? scopeNode.Scope() : enclosingScope;

    private static ILogixDataPoint ToDataPoint(IDataPointNode dataPointNode, TagScope scope) => dataPointNode switch
    {
        DIntNode dInt => new DIntDataPoint(scope.Qualify(dInt.TagName), dInt.PollFrequency, dInt.Channels),
        SIntNode sInt => new SIntDataPoint(scope.Qualify(sInt.TagName), sInt.PollFrequency, sInt.Channels),
        LIntNode lInt => new LIntDataPoint(scope.Qualify(lInt.TagName), lInt.PollFrequency, lInt.Channels),
        USIntNode usInt => new USIntDataPoint(scope.Qualify(usInt.TagName), usInt.PollFrequency, usInt.Channels),
        UIntNode uInt => new UIntDataPoint(scope.Qualify(uInt.TagName), uInt.PollFrequency, uInt.Channels),
        UDIntNode uDInt => new UDIntDataPoint(
            scope.Qualify(uDInt.TagName), uDInt.PollFrequency, uDInt.Channels),
        ULIntNode uLInt => new ULIntDataPoint(
            scope.Qualify(uLInt.TagName), uLInt.PollFrequency, uLInt.Channels),
        BoolNode flag => new BoolDataPoint(scope.Qualify(flag.TagName), flag.PollFrequency, flag.Channels),
        IntNode integer => new IntDataPoint(
            scope.Qualify(integer.TagName), integer.PollFrequency, integer.Channels),
        RealNode real => new RealDataPoint(scope.Qualify(real.TagName), real.PollFrequency, real.Channels),
        LRealNode lReal => new LRealDataPoint(scope.Qualify(lReal.TagName), lReal.PollFrequency, lReal.Channels),
        StringNode text => new StringDataPoint(
            scope.Qualify(text.TagName), text.PollFrequency, text.Channels, text.MaxLength),
        SIntArrayNode samples => new SIntArrayDataPoint(
            scope.Qualify(samples.TagName), samples.PollFrequency, samples.Channels,
            samples.ElementCount),
        IntArrayNode readings => new IntArrayDataPoint(
            scope.Qualify(readings.TagName), readings.PollFrequency, readings.Channels,
            readings.ElementCount),
        DIntArrayNode totals => new DIntArrayDataPoint(
            scope.Qualify(totals.TagName), totals.PollFrequency, totals.Channels,
            totals.ElementCount),
        LIntArrayNode timestamps => new LIntArrayDataPoint(
            scope.Qualify(timestamps.TagName), timestamps.PollFrequency, timestamps.Channels,
            timestamps.ElementCount),
        USIntArrayNode pressures => new USIntArrayDataPoint(
            scope.Qualify(pressures.TagName), pressures.PollFrequency, pressures.Channels,
            pressures.ElementCount),
        UIntArrayNode speeds => new UIntArrayDataPoint(
            scope.Qualify(speeds.TagName), speeds.PollFrequency, speeds.Channels,
            speeds.ElementCount),
        UDIntArrayNode runtimes => new UDIntArrayDataPoint(
            scope.Qualify(runtimes.TagName), runtimes.PollFrequency, runtimes.Channels,
            runtimes.ElementCount),
        ULIntArrayNode cycleCounts => new ULIntArrayDataPoint(
            scope.Qualify(cycleCounts.TagName), cycleCounts.PollFrequency, cycleCounts.Channels,
            cycleCounts.ElementCount),
        RealArrayNode temperatures => new RealArrayDataPoint(
            scope.Qualify(temperatures.TagName), temperatures.PollFrequency, temperatures.Channels,
            temperatures.ElementCount),
        LRealArrayNode positions => new LRealArrayDataPoint(
            scope.Qualify(positions.TagName), positions.PollFrequency, positions.Channels,
            positions.ElementCount),
        // Unreachable from a manifest: a node this switch does not name has a node mapper and no data
        // point behind it, which only a half-finished type slice produces.
        _ => throw new NotSupportedException(
            $"Unsupported Logix data point node type '{dataPointNode.GetType().Name}'."),
    };
}
