using System.Globalization;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration.Templates;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// Browses the controller's symbol table over the <see cref="ILogixTagAccess"/> seam: the <c>@tags</c>
/// listing, then each program's, then every template a structured tag names, assembled into a
/// <see cref="TagDefinitions"/> keyed by qualified name (<c>Program:Main.Count</c>) and by template id.
/// Nested programs are not walked, and no browse handle outlives the read it was opened for.
/// </summary>
internal sealed class TagDefinitionsLoader(ILogixTagAccessFactory accessFactory) : ITagDefinitionsLoader
{
    private static readonly TagAddress ControllerListing = new("@tags");
    private const string ProgramPrefix = "Program:";
    private const string TemplatePrefix = "@udt/";

    /// <inheritdoc />
    public async Task<TagDefinitions> LoadAsync(CancellationToken cancellationToken)
    {
        // The controller listing holds every controller-scoped tag, complete, plus one entry per program.
        // A program's own tags are not in it: each program has a listing of its own.
        var controllerListing = await ReadListingAsync(ControllerListing, cancellationToken).ConfigureAwait(false);

        var tags = new List<TagDefinition>(controllerListing);
        foreach (var program in controllerListing.Where(IsProgram))
        {
            tags.AddRange(await ReadProgramTagsAsync(program, cancellationToken).ConfigureAwait(false));
        }

        // A listing names a structure only by its template id; the template itself is one more read.
        var templates = await ReadTemplatesAsync(tags, cancellationToken).ConfigureAwait(false);

        return new TagDefinitions(ByTagAddress(tags), templates);
    }

    private static bool IsProgram(TagDefinition entry) =>
        entry.TagAddress.Value.StartsWith(ProgramPrefix, StringComparison.Ordinal);

    // A program tag is configured and looked up as Program:Main.Count, so that is the name it leaves with.
    private async Task<IEnumerable<TagDefinition>> ReadProgramTagsAsync(
        TagDefinition program, CancellationToken cancellationToken)
    {
        var programListing = await ReadListingAsync(
            new TagAddress($"{program.TagAddress.Value}.@tags"), cancellationToken).ConfigureAwait(false);

        return programListing.Select(tag =>
            tag with { TagAddress = new TagAddress($"{program.TagAddress.Value}.{tag.TagAddress.Value}") });
    }

    // An entry without a name is a truncated final entry, not a tag.
    private async Task<IReadOnlyList<TagDefinition>> ReadListingAsync(
        TagAddress listingAddress, CancellationToken cancellationToken)
    {
        var listing = await ReadSchemaTagAsync(listingAddress, cancellationToken).ConfigureAwait(false);

        return TagsDecoder.Decode(listing.Span).Where(entry => entry.TagAddress.Value.Length > 0).ToList();
    }

    // Logix resolves tag names case-insensitively, so the lookup does too; of two entries with one name,
    // the first one listed is kept.
    private static Dictionary<TagAddress, TagDefinition> ByTagAddress(IEnumerable<TagDefinition> tags)
    {
        var tagsByAddress = new Dictionary<TagAddress, TagDefinition>(TagAddress.CaseInsensitiveComparer);
        foreach (var tag in tags)
        {
            tagsByAddress.TryAdd(tag.TagAddress, tag);
        }

        return tagsByAddress;
    }

    private async Task<Dictionary<TemplateId, TemplateDefinition>> ReadTemplatesAsync(
        IEnumerable<TagDefinition> tags, CancellationToken cancellationToken)
    {
        var templatesById = new Dictionary<TemplateId, TemplateDefinition>();

        foreach (var templateId in TemplateIdsOf(tags))
        {
            await ReadTemplateWithNestedTemplatesAsync(templateId, templatesById, cancellationToken).ConfigureAwait(false);
        }

        return templatesById;
    }

    // A member can be a structure itself, so its template is read as well. A template already read is
    // skipped, which is what keeps a template two tags share at one read and a self-referencing one
    // from recursing forever.
    private async Task ReadTemplateWithNestedTemplatesAsync(
        TemplateId templateId,
        Dictionary<TemplateId, TemplateDefinition> templatesById,
        CancellationToken cancellationToken)
    {
        if (templatesById.ContainsKey(templateId))
        {
            return;
        }

        var template = await ReadTemplateAsync(templateId, cancellationToken).ConfigureAwait(false);
        templatesById.Add(templateId, template);

        foreach (var nestedTemplateId in TemplateIdsOf(template.Members))
        {
            await ReadTemplateWithNestedTemplatesAsync(nestedTemplateId, templatesById, cancellationToken).ConfigureAwait(false);
        }
    }

    private static IEnumerable<TemplateId> TemplateIdsOf(IEnumerable<TagDefinition> tags) =>
        tags.Select(tag => tag.TemplateId).OfType<TemplateId>();

    private static IEnumerable<TemplateId> TemplateIdsOf(IEnumerable<TemplateMember> members) =>
        members.Select(member => member.TemplateId).OfType<TemplateId>();

    private async Task<TemplateDefinition> ReadTemplateAsync(TemplateId templateId, CancellationToken cancellationToken)
    {
        var templateAddress = new TagAddress(TemplatePrefix + templateId.Value.ToString(CultureInfo.InvariantCulture));
        var template = await ReadSchemaTagAsync(templateAddress, cancellationToken).ConfigureAwait(false);

        try
        {
            return TemplateDecoder.Decode(template.Span);
        }
        catch (FormatException ex)
        {
            throw new DataRetrievalException(
                $"The controller's template read via '{templateAddress}' could not be decoded: {ex.Message}", ex);
        }
    }

    // Every browse read is the same read of a schema tag; only the decoding of what comes back differs.
    private async Task<ReadOnlyMemory<byte>> ReadSchemaTagAsync(
        TagAddress schemaTagAddress, CancellationToken cancellationToken)
    {
        using var access = accessFactory.CreateForSchemaTag(schemaTagAddress);
        var readResult = await access.ReadAsync(cancellationToken).ConfigureAwait(false);

        if (!readResult.Succeeded)
        {
            throw new DataRetrievalException(
                $"Could not browse the controller symbol table via '{schemaTagAddress}': {readResult.Error}");
        }

        return readResult.Buffer;
    }
}
