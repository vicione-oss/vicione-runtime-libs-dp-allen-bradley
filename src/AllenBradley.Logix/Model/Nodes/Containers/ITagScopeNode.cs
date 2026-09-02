using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
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
}
