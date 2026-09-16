using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Booleans.BoolArray;
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
    /// <summary>
    /// The tree is three levels deep and no deeper: a scope container under the device, an array
    /// container under a scope, and tag nodes under either. The walk names each level, and composes
    /// the <see cref="TagPath"/> where the parts are in scope.
    /// </summary>
    public IReadOnlyList<ILogixDataPoint> ToDataPoints(DeviceNode deviceNode) =>
    [
        .. from scope in deviceNode.ConfigurationNodes.OfType<ILogixContainerNode>()
        let program = ProgramOf(scope)
        from dataPoint in ScopedTags(program, scope).Concat(ArrayElements(program, scope))
        select dataPoint,
    ];

    /// <inheritdoc />
    public LogixDataPointGroup CreateGroup(PollFrequency pollFrequency, IReadOnlyList<ILogixDataPoint> dataPoints) =>
        new(pollFrequency, dataPoints);

    private static ProgramName? ProgramOf(ILogixContainerNode scope) => scope switch
    {
        ProgramTagsNode program => program.ProgramName,
        ControllerTagsNode => null,
        _ => throw new NotSupportedException($"'{scope.GetType().Name}' is no tag scope."),
    };

    private static IEnumerable<ILogixDataPoint> ScopedTags(ProgramName? program, ILogixContainerNode scope) =>
        from tagNode in scope.DataPointNodes.OfType<ILogixDataPointNode>()
        select ToDataPoint(new TagPath(program, tagNode.TagName, Element: null), tagNode);

    // Under an array container a node's tag name is the subscript, and the array's name is the tag.
    private static IEnumerable<ILogixDataPoint> ArrayElements(ProgramName? program, ILogixContainerNode scope) =>
        from array in scope.ConfigurationNodes.OfType<ArrayContainerNode>()
        from elementNode in array.DataPointNodes.OfType<ILogixDataPointNode>()
        select ToDataPoint(new TagPath(program, array.TagName, elementNode.TagName.ToElementIndex()), elementNode);

    private static ILogixDataPoint ToDataPoint(TagPath tagPath, ILogixDataPointNode dataPointNode)
    {
        var pollFrequency = dataPointNode.PollFrequency;
        var channels = dataPointNode.Channels;

        return dataPointNode switch
        {
            BoolNode => new BoolDataPoint(tagPath, pollFrequency, channels),
            SIntNode => new SIntDataPoint(tagPath, pollFrequency, channels),
            IntNode => new IntDataPoint(tagPath, pollFrequency, channels),
            DIntNode => new DIntDataPoint(tagPath, pollFrequency, channels),
            LIntNode => new LIntDataPoint(tagPath, pollFrequency, channels),
            USIntNode => new USIntDataPoint(tagPath, pollFrequency, channels),
            UIntNode => new UIntDataPoint(tagPath, pollFrequency, channels),
            UDIntNode => new UDIntDataPoint(tagPath, pollFrequency, channels),
            ULIntNode => new ULIntDataPoint(tagPath, pollFrequency, channels),
            RealNode => new RealDataPoint(tagPath, pollFrequency, channels),
            LRealNode => new LRealDataPoint(tagPath, pollFrequency, channels),
            StringNode stringNode => new StringDataPoint(tagPath, pollFrequency, channels, stringNode.MaxLength),
            LogixArrayDataPointNode arrayNode => ToArrayDataPoint(arrayNode, tagPath),
            _ => throw UnsupportedNode(dataPointNode),
        };
    }

    private static ILogixDataPoint ToArrayDataPoint(LogixArrayDataPointNode arrayDataPointNode, TagPath tagPath)
    {
        var pollFrequency = arrayDataPointNode.PollFrequency;
        var channels = arrayDataPointNode.Channels;
        var elementCount = arrayDataPointNode.ElementCount;

        return arrayDataPointNode switch
        {
            BoolArrayDataPointNode => new BoolArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            SIntArrayDataPointNode => new SIntArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            IntArrayDataPointNode => new IntArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            DIntArrayDataPointNode => new DIntArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            LIntArrayDataPointNode => new LIntArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            UsIntArrayDataPointNode => new USIntArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            UIntArrayDataPointNode => new UIntArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            UdIntArrayDataPointNode => new UDIntArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            UlIntArrayDataPointNode => new ULIntArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            RealArrayDataPointNode => new RealArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            LRealArrayDataPointNode => new LRealArrayDataPoint(tagPath, pollFrequency, channels, elementCount),
            _ => throw UnsupportedNode(arrayDataPointNode),
        };
    }

    // Unreachable from a manifest: a node the switches do not name has a node mapper and no data
    // point behind it, which only a half-finished type slice produces.
    private static NotSupportedException UnsupportedNode(ILogixDataPointNode dataPointNode) =>
        new($"Unsupported Logix data point node type '{dataPointNode.GetType().Name}'.");
}
