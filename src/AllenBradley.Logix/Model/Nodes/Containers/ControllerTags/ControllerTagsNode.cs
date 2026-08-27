using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;

/// <summary>
/// The controller-scope tag container. It carries no properties because controller scope contributes
/// no segment to a tag address: a tag configured under it addresses itself. Program scope is the one
/// that prefixes — <c>Program:MainProgram.Count</c> — and gets its own node when it arrives.
/// </summary>
/// <param name="OriginalNode">The untyped node this was mapped from.</param>
/// <param name="ControllerName">
/// The controller's name in the editor tree. A placeholder — nothing reads it, and it reaches no tag
/// address. See <see cref="ControllerNamePropertyName"/>.
/// </param>
public sealed record ControllerTagsNode(LinkedNode OriginalNode, string ControllerName) : IBranchConfigurationNode
{
    /// <summary>The manifest's <c>MappingId</c> for this node.</summary>
    public const string LinkedNodeTypeId = "ControllerTags";

    /// <summary>
    /// The manifest property carrying <see cref="ControllerName"/>.
    /// </summary>
    /// <remarks>
    /// The property exists so the YAML consistency test has a branch-node case to run: xUnit fails a
    /// theory that discovers none, and controller scope contributes nothing to a tag address, so it has
    /// no property of its own. Delete it — and the node's <see cref="ControllerName"/> with it — once
    /// the base test sets <c>Theory.SkipTestWithoutData</c>, or once program scope brings a branch node
    /// carrying a real <c>ProgramName</c>.
    /// </remarks>
    public const string ControllerNamePropertyName = nameof(ControllerName);

    /// <inheritdoc />
    public IConfigurationNode? ParentConfigurationNode { get; set; }

    /// <inheritdoc />
    public List<IConfigurationNode> ConfigurationNodes { get; } = [];

    /// <inheritdoc />
    public List<IDataPointNode> DataPointNodes { get; } = [];

    /// <summary>Nothing nests inside controller scope yet — structures and programs are later slices.</summary>
    public bool CanBeAdded(IConfigurationNode configurationNode) => false;

    /// <inheritdoc />
    public bool CanBeAdded(IDataPointNode dataPointNode) => true;
}
