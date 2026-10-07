using FluentValidation;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.Mapping;

/// <summary>
/// Checks a data file's number: 0 to 2 are the output, input and status files, which are not typed by
/// the user, and 255 is the last file an SLC 500 or MicroLogix has.
/// </summary>
internal sealed class FileNumberPropertyValidator : AbstractValidator<LinkedNode>
{
    internal const ushort FirstUserFile = 3;

    internal const ushort LastFile = 255;

    public FileNumberPropertyValidator()
    {
        RuleFor(static node => node)
            .Must(static node => node.HasPropertyOfType<ushort>(DataFileNode.FileNumberPropertyName))
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{DataFileNode.FileNumberPropertyName}' is required and must be a number.")
            .WithName(DataFileNode.FileNumberPropertyName);

        RuleFor(static node => node)
            .Must(static node =>
                node.GetRequiredPropertyValue<ushort>(DataFileNode.FileNumberPropertyName) is >= FirstUserFile and <= LastFile)
            .WithMessage(static node =>
                $"Node '{node.Name}' ({node.DesignId}): property '{DataFileNode.FileNumberPropertyName}' must be between {FirstUserFile} and {LastFile}.")
            .When(static node => node.HasPropertyOfType<ushort>(DataFileNode.FileNumberPropertyName))
            .WithName(DataFileNode.FileNumberPropertyName);
    }
}
