using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// Browses the controller's symbol table over the <see cref="ILogixTagAccess"/> seam: the <c>@tags</c>
/// directory, then each program's, assembled into a <see cref="TagDefinitions"/> keyed by qualified name
/// (<c>Program:Main.Count</c>). Nested programs are not walked, and no listing handle outlives the browse.
/// </summary>
internal sealed class TagDefinitionsLoader(ILogixTagAccessFactory accessFactory) : ITagDefinitionsLoader
{
    private static readonly TagAddress ControllerTags = new("@tags");
    private const string ProgramPrefix = "Program:";

    /// <inheritdoc />
    public async Task<TagDefinitions> LoadAsync(CancellationToken cancellationToken)
    {
        var declarationsByTagName = new Dictionary<TagAddress, TagDefinition>(TagAddress.CaseInsensitiveComparer);

        var controllerTags = await ReadDirectoryAsync(ControllerTags, cancellationToken).ConfigureAwait(false);
        AddTags(declarationsByTagName, controllerTags, programScope: null);

        foreach (var program in controllerTags.Where(IsProgram))
        {
            var programTags = await ReadDirectoryAsync(new TagAddress($"{program.TagAddress.Value}.@tags"), cancellationToken)
                .ConfigureAwait(false);
            AddTags(declarationsByTagName, programTags, programScope: program.TagAddress);
        }

        return new TagDefinitions(declarationsByTagName);
    }

    private async Task<IReadOnlyList<TagDefinition>> ReadDirectoryAsync(
        TagAddress schemaTagAddress, CancellationToken cancellationToken)
    {
        using var access = accessFactory.CreateForSchemaTag(schemaTagAddress);
        var readResult = await access.ReadAsync(cancellationToken).ConfigureAwait(false);

        if (!readResult.Succeeded)
        {
            throw new DataRetrievalException(
                $"Could not browse the controller symbol table via '{schemaTagAddress}': {readResult.Error}");
        }

        return TagsDecoder.Decode(readResult.Buffer.Span);
    }

    private static bool IsProgram(TagDefinition declaration) =>
        declaration.TagAddress.Value.StartsWith(ProgramPrefix, StringComparison.Ordinal);

    private static void AddTags(
        Dictionary<TagAddress, TagDefinition> declarationsByTagName,
        IReadOnlyList<TagDefinition> tags,
        TagAddress? programScope)
    {
        foreach (var tag in tags)
        {
            if (tag.TagAddress.Value.Length == 0)
            {
                continue;
            }

            // Keyed by the qualified name so it matches a configured address, and the declaration is
            // rewritten to carry that same name.
            if (programScope is { } scope)
            {
                var qualifiedName = new TagAddress($"{scope.Value}.{tag.TagAddress.Value}");
                declarationsByTagName.TryAdd(qualifiedName, tag with { TagAddress = qualifiedName });
            }
            else
            {
                declarationsByTagName.TryAdd(tag.TagAddress, tag);
            }
        }
    }
}
