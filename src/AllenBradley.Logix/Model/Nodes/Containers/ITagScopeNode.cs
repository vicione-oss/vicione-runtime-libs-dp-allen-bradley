using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;

/// <summary>
/// A container standing for one of the controller's tag scopes: it says what the addresses inside it
/// carry, and which controller generation it was configured against. The tree walk asks the first; the
/// device asks the second when the container is attached to it.
/// </summary>
/// <remarks>
/// Both scopes answer both questions, which is why they are one interface rather than two type tests.
/// The leaves below stay ignorant of either — a tag is configured as a bare name whatever it hangs under.
/// </remarks>
public interface ITagScopeNode : IBranchConfigurationNode
{
    /// <summary>
    /// The generation of the controller these tags are configured against, from the container's own node
    /// type. It decides which types may hang off this container, and whether the device will take it at
    /// all. The node type it came from is on <c>OriginalNode</c>, which every branch node carries.
    /// </summary>
    LogixGeneration Generation { get; }

    /// <summary>The segment every tag address below this container carries.</summary>
    /// <remarks>
    /// A method rather than a property, because a branch node's properties are held against the ones the
    /// manifest declares and this one is composed rather than configured.
    /// </remarks>
    TagScope Scope();

    /// <summary>
    /// Any tag whose type the controller has, which the container answers by holding the type's
    /// <see cref="ILogixScalarNode.MinimumGeneration"/> against its own generation.
    /// </summary>
    /// <remarks>
    /// Implemented here rather than by each scope, because scope makes no difference to it: a type the
    /// controller has not got is missing from controller scope and from every program alike. Implementing
    /// it here is also what stops the next container from having to remember the rule.
    /// <para>
    /// It throws where it might return false, the way
    /// <see cref="Device.DeviceNode.CanBeAdded(IConfigurationNode)"/> does. The manifest keeps the editor
    /// from offering a type the controller lacks — a <c>ControllerTags5X70</c> node lists no <c>LReal</c>
    /// among its children — so a configuration that reaches here holding one was not built through the
    /// editor. A silent refusal would leave an integrator with a tag that vanished; the alternative to the
    /// throw is a tag address the controller cannot resolve reaching the poll.
    /// </para>
    /// </remarks>
    bool IConfigurationNode.CanBeAdded(IDataPointNode dataPointNode)
    {
        // Every data point node is a scalar for now; structures and arrays are later slices, and they
        // will carry a minimum generation of their own through the same interface.
        if (dataPointNode is ILogixScalarNode scalarNode && scalarNode.MinimumGeneration > Generation)
        {
            throw new InvalidConfigurationException(
                $"'{dataPointNode.OriginalNode.DesignId}' is not a data type of a {Generation} controller.");
        }

        return true;
    }
}
