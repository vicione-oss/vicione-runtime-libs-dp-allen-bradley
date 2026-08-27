using FluentValidation.Results;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags.Mapping;

/// <summary>
/// Maps the controller-scope container. It reads one property and validates none: the container's
/// <see cref="ControllerTagsNode.ControllerName"/> is a placeholder that reaches no tag address.
/// </summary>
internal sealed class ControllerTagsNodeMapper : IBranchConfigurationNodeMapper<ControllerTagsNode>
{
    /// <inheritdoc />
    public string TargetLinkedNodeTypeId => ControllerTagsNode.LinkedNodeTypeId;

    /// <inheritdoc />
    public ControllerTagsNode Map(LinkedNode node) => new(
        node,
        node.GetRequiredPropertyValue<string>(ControllerTagsNode.ControllerNamePropertyName));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => new();
}
