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
    private static readonly TagName ControllerTags = new("@tags");
    private const string ProgramPrefix = "Program:";

    /// <inheritdoc />
    public async Task<TagDefinitions> LoadAsync(CancellationToken cancellationToken)
    {
        var declarationsByTagName = new Dictionary<TagName, TagDefinition>(TagName.CaseInsensitiveComparer);

        var controllerTags = await ReadDirectoryAsync(ControllerTags, cancellationToken).ConfigureAwait(false);
        AddTags(declarationsByTagName, controllerTags, programScope: null);

        foreach (var program in controllerTags.Where(IsProgram))
        {
            var programTags = await ReadDirectoryAsync(new TagName($"{program.TagName.Value}.@tags"), cancellationToken)
                .ConfigureAwait(false);
            AddTags(declarationsByTagName, programTags, programScope: program.TagName);
        }

        return new TagDefinitions(declarationsByTagName);
    }

    private async Task<IReadOnlyList<TagDefinition>> ReadDirectoryAsync(
        TagName schemaTagName, CancellationToken cancellationToken)
    {
        using var access = accessFactory.CreateForSchemaTag(schemaTagName);
        var readResult = await access.ReadAsync(cancellationToken).ConfigureAwait(false);

        if (!readResult.Succeeded)
        {
            throw new DataRetrievalException(
                $"Could not browse the controller symbol table via '{schemaTagName}': {readResult.Error}");
        }

        return TagsDecoder.Decode(readResult.Buffer.Span);
    }

    private static bool IsProgram(TagDefinition declaration) =>
        declaration.TagName.Value.StartsWith(ProgramPrefix, StringComparison.Ordinal);

    private static void AddTags(
        Dictionary<TagName, TagDefinition> declarationsByTagName,
        IReadOnlyList<TagDefinition> tags,
        TagName? programScope)
    {
        foreach (var tag in tags)
        {
            if (tag.TagName.Value.Length == 0)
            {
                continue;
            }

            // Keyed by the qualified name so it matches a configured address, and the declaration is
            // rewritten to carry that same name.
            if (programScope is { } scope)
            {
                var qualifiedName = new TagName($"{scope.Value}.{tag.TagName.Value}");
                declarationsByTagName.TryAdd(qualifiedName, tag with { TagName = qualifiedName });
            }
            else
            {
                declarationsByTagName.TryAdd(tag.TagName, tag);
            }
        }
    }
}
