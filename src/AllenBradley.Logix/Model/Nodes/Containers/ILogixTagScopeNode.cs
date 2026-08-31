using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;

/// <summary>
/// A container standing for one of the controller's tag scopes, which can say what the addresses inside
/// it carry. The tree walk asks; the leaves below stay ignorant of which scope they hang under.
/// </summary>
/// <remarks>
/// A method rather than a property, because a branch node's properties are held against the ones the
/// manifest declares and this one is composed rather than configured.
/// </remarks>
public interface ILogixTagScopeNode : IBranchConfigurationNode
{
    /// <summary>The segment every tag address below this container carries.</summary>
    TagScope Scope();
}
