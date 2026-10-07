using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles;

internal abstract record LegacyContainerNode(LinkedNode OriginalNode) : ILegacyContainerNode
{
    public bool CanBeAdded(IConfigurationNode configurationNode)
    {
        if (configurationNode is not ILegacyContainerNode legacyContainerNode) return false;

        return CanBeAdded(legacyContainerNode);
    }

    internal abstract bool CanBeAdded(ILegacyContainerNode legacyContainerNode);

    public bool CanBeAdded(IDataPointNode dataPointNode)
    {
        if (dataPointNode is not ILegacyDataPointNode legacyDataPointNode) return false;

        return CanBeAdded(legacyDataPointNode);
    }

    internal abstract bool CanBeAdded(ILegacyDataPointNode legacyDataPointNode);

    public IConfigurationNode? ParentConfigurationNode { get; set; }
    public List<IConfigurationNode> ConfigurationNodes { get; } = [];
    public List<IDataPointNode> DataPointNodes { get; } = [];
}
