using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;

/// <summary>
/// A container standing for one of the controller's tag scopes: it says what the addresses inside it
/// carry, and which controller generation it was configured against. The tree walk asks the first; the
/// device asks the second when the container is attached to it.
/// </summary>
public interface ITagScopeNode : IBranchConfigurationNode
{
    /// <summary>
    /// The generation of the controller these tags are configured against, from the container's own node
    /// type. It decides which types may hang off this container, and whether the device will take it at
    /// all. The node type it came from is on <c>OriginalNode</c>, which every branch node carries.
    /// </summary>
    LogixGeneration Generation { get; }

    /// <summary>
    /// The segment every tag address below this container carries. A method rather than a property, because a
    /// branch node's properties are held against the manifest.
    /// </summary>
    TagScope Scope();

    /// <summary>
    /// Any tag whose type the controller has, which a container answers by holding the type's
    /// <see cref="ILogixTagNode.MinimumGeneration"/> against <paramref name="generation"/>.
    /// Throws rather than returns <c>false</c>: the manifest keeps the editor from offering such a type, so a
    /// configuration holding one was not built through the editor and must not silently lose a tag.
    /// Shared here rather than defaulted on the interface, so a container exposes <c>CanBeAdded</c> as an
    /// ordinary method a caller can invoke without first casting up to <see cref="IConfigurationNode"/>.
    /// </summary>
    protected static bool CanHold(IDataPointNode dataPointNode, LogixGeneration generation)
    {
        // Shape does not come into it: a scalar and an array both name their minimum generation through
        // ILogixTagNode, and a structure will when there is one.
        if (dataPointNode is ILogixTagNode tagNode && tagNode.MinimumGeneration > generation)
        {
            throw new InvalidConfigurationException(
                $"'{dataPointNode.OriginalNode.DesignId}' is not a data type of a {generation} controller.");
        }

        return true;
    }
}
