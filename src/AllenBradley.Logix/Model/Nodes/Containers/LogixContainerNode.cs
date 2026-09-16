using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;

internal abstract record LogixContainerNode(LinkedNode OriginalNode) : ILogixContainerNode
{
    public bool CanBeAdded(IConfigurationNode configurationNode)
    {
        if (configurationNode is not ILogixContainerNode logixBranchNode) return false;

        return CanBeAdded(logixBranchNode);
    }

    internal abstract bool CanBeAdded(ILogixContainerNode logixContainerNode);


    public bool CanBeAdded(IDataPointNode dataPointNode)
    {
        if (dataPointNode is not ILogixDataPointNode logixDataPointNode) return false;

        return CanBeAdded(logixDataPointNode);
    }

    internal abstract bool CanBeAdded(ILogixDataPointNode logixDataPointNode);

    public IConfigurationNode? ParentConfigurationNode { get; set; }
    public List<IConfigurationNode> ConfigurationNodes { get; } = [];
    public List<IDataPointNode> DataPointNodes { get; } = [];
}
