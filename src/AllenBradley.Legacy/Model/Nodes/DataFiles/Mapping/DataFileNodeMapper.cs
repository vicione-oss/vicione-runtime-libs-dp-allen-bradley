using FluentValidation.Results;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

/// <summary>
/// What every data file mapper does whatever the file's type: validate the configured node and read the
/// file number off it. A concrete mapper adds only the <c>LinkedNodeTypeId</c> it claims and the
/// constructor call.
/// </summary>
/// <typeparam name="TNode">The node the mapper produces.</typeparam>
internal abstract class DataFileNodeMapper<TNode> : IBranchConfigurationNodeMapper<TNode>
    where TNode : DataFileNode
{
    private static readonly FileNumberPropertyValidator Validator = new();

    /// <inheritdoc />
    public abstract string TargetLinkedNodeTypeId { get; }

    /// <inheritdoc />
    public TNode Map(LinkedNode node) => CreateNode(node, GetFileNumber(node));

    /// <inheritdoc />
    public ValidationResult Validate(LinkedNode linkedNode) => Validator.Validate(linkedNode);

    protected abstract TNode CreateNode(LinkedNode originalNode, FileNumber fileNumber);

    private static FileNumber GetFileNumber(LinkedNode node) =>
        new(node.GetRequiredPropertyValue<ushort>(DataFileNode.FileNumberPropertyName));
}
