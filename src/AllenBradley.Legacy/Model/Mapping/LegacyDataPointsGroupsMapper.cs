using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Integer;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Mapping;

/// <summary>
/// Turns a configured node tree into the runtime data points both ports work from, grouped by poll
/// frequency for the incoming one. Each element takes its file number from the data file it sits in.
/// </summary>
internal sealed class LegacyDataPointsGroupsMapper : IDataPointGroupsMapper<ILegacyDataPoint, LegacyDataPointGroup,
    DeviceNode, LegacyCommunication>
{
    /// <inheritdoc />
    public IReadOnlyList<ILegacyDataPoint> ToDataPoints(DeviceNode deviceNode) =>
    [
        .. from file in deviceNode.ConfigurationNodes.OfType<DataFileNode>()
        from element in file.DataPointNodes.OfType<ILegacyDataPointNode>()
        select ToDataPoint(file.FileNumber, element),
    ];

    /// <inheritdoc />
    public LegacyDataPointGroup CreateGroup(PollFrequency pollFrequency, IReadOnlyList<ILegacyDataPoint> dataPoints) =>
        new(pollFrequency, dataPoints);

    private static ILegacyDataPoint ToDataPoint(FileNumber fileNumber, ILegacyDataPointNode element) => element switch
    {
        IntegerNode => new IntegerDataPoint(fileNumber, element.ElementNumber, element.PollFrequency, element.Channels),
        _ => throw UnsupportedNode(element),
    };

    // Unreachable from a manifest: a node the switch does not name has a node mapper and no data point
    // behind it, which only a half-finished type slice produces.
    private static NotSupportedException UnsupportedNode(ILegacyDataPointNode element) =>
        new($"Unsupported Legacy data point node type '{element.GetType().Name}'.");
}
