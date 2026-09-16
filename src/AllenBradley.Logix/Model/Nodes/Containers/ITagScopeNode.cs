using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;

/// <summary>
/// A container standing for one of the controller's tag scopes: the segment the addresses inside it
/// carry, and the controller generation it was configured against.
/// </summary>
public interface ITagScopeNode : IBranchConfigurationNode
{
    /// <summary>
    /// The generation of the controller these tags are configured against, taken from the container's own
    /// node type because the engine assembles a container's data points before attaching it to a device.
    /// </summary>
    LogixGeneration Generation { get; }

    /// <summary>
    /// The segment every tag address below this container carries. A method rather than a property, because a
    /// branch node's properties are held against the manifest.
    /// </summary>
    TagScope Scope();

    /// <summary>
    /// Whether <paramref name="dataPointNode"/>'s type is one a <paramref name="generation"/> controller
    /// has. Throws rather than returns <c>false</c>: the editor cannot offer such a type, so a
    /// configuration holding one must not silently lose the tag.
    /// </summary>
    protected static bool CanHold(IDataPointNode dataPointNode, LogixGeneration generation)
    {
        if (dataPointNode is ILogixDataPointNode tagNode && tagNode.MinimumGeneration > generation)
        {
            throw new InvalidConfigurationException(
                $"'{dataPointNode.OriginalNode.DesignId}' is not a data type of a {generation} controller.");
        }

        return true;
    }
}
