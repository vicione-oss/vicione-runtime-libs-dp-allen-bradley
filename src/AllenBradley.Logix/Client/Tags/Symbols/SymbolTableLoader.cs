using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.SymbolTypes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.TagsListing;
using System.Globalization;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols.Templates;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;

/// <summary>
/// Browses the controller's symbol table over the <see cref="ILogixTagAccess"/> seam: the <c>@tags</c>
/// listing, then each program's, then every template a structured tag names, assembled into a
/// <see cref="SymbolTable"/> keyed by qualified name (<c>Program:Main.Count</c>) and by template id.
/// Nested programs are not walked, and no browse handle outlives the read it was opened for.
/// </summary>
internal sealed class SymbolTableLoader(ILogixTagAccessFactory accessFactory) : ISymbolTableLoader
{
    private const string ProgramPrefix = "Program:";
    private const string TemplatePrefix = "@udt/";
    private const string TagsListingAddress = "@tags";

    /// <inheritdoc />
    public async Task<SymbolTable> LoadAsync(CancellationToken cancellationToken)
    {
        var controllerTags = await GetControllerTags(cancellationToken).ConfigureAwait(false);
        var programTags = await GetTagsOfEachProgram(controllerTags, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<ListedTag> allTags = [.. controllerTags, .. programTags];
        var templates = await GetTemplateDefinitionsUsedBy(allTags, cancellationToken).ConfigureAwait(false);
        return new SymbolTable(allTags, templates);
    }

    // The controller listing names each program but holds none of its tags.
    private async Task<IReadOnlyList<ListedTag>> GetTagsOfEachProgram(
        IEnumerable<ListedTag> controllerTags, CancellationToken cancellationToken)
    {
        var tagsPerProgram = new List<IEnumerable<ListedTag>>();

        foreach (var program in controllerTags.Where(IsProgram))
        {
            tagsPerProgram.Add(await GetProgramTags(program, cancellationToken).ConfigureAwait(false));
        }

        return [.. tagsPerProgram.SelectMany(tags => tags)];
    }

    private async Task<IReadOnlyList<ListedTag>> GetControllerTags(CancellationToken cancellationToken) =>
        await ReadListedTagAsync(new TagAddress(TagsListingAddress), cancellationToken).ConfigureAwait(false);

    private static bool IsProgram(ListedTag entry) =>
        entry.TagAddress.Value.StartsWith(ProgramPrefix, StringComparison.Ordinal);

    private async Task<IEnumerable<ListedTag>> GetProgramTags(ListedTag program,
        CancellationToken cancellationToken)
    {
        var programTagsAddress = new TagAddress($"{program.TagAddress.Value}.{TagsListingAddress}");

        var programListing = await ReadListedTagAsync(programTagsAddress, cancellationToken).ConfigureAwait(false);

        return programListing.Select(tag =>
            tag with { TagAddress = new TagAddress($"{program.TagAddress.Value}.{tag.TagAddress.Value}") });
    }

    private async Task<IReadOnlyList<ListedTag>> ReadListedTagAsync(
        TagAddress listingAddress, CancellationToken cancellationToken)
    {
        var listing = await ReadRawTagValueAsync(listingAddress, cancellationToken).ConfigureAwait(false);

        return [.. TagsDecoder.Decode(listing.Span).Where(IsNotTruncatedFinalEntry)];
    }

    private static bool IsNotTruncatedFinalEntry(ListedTag entry) => entry.TagAddress.Value.Length > 0;

    private async Task<IReadOnlyList<TemplateDefinition>> GetTemplateDefinitionsUsedBy(
        IReadOnlyList<ListedTag> tags, CancellationToken cancellationToken) =>
        await ReadTemplatesAsync(TemplateIdsOf(tags), readSoFar: [], cancellationToken).ConfigureAwait(false);

    // Each template is read together with the templates its members use, so what one read returns is
    // the starting point of the next.
    private async Task<IReadOnlyList<TemplateDefinition>> ReadTemplatesAsync(
        IEnumerable<TemplateId> templateIds,
        IReadOnlyList<TemplateDefinition> readSoFar,
        CancellationToken cancellationToken)
    {
        var read = readSoFar;

        foreach (var templateId in templateIds)
        {
            read = await ReadTemplateWithNestedTemplatesAsync(templateId, read, cancellationToken).ConfigureAwait(false);
        }

        return read;
    }

    // A template already read is not read again, which also stops a self-referencing template from
    // recursing forever.
    private async Task<IReadOnlyList<TemplateDefinition>> ReadTemplateWithNestedTemplatesAsync(
        TemplateId templateId,
        IReadOnlyList<TemplateDefinition> readSoFar,
        CancellationToken cancellationToken)
    {
        if (readSoFar.Any(template => template.Id == templateId))
        {
            return readSoFar;
        }

        var template = await ReadTemplateAsync(templateId, cancellationToken).ConfigureAwait(false);

        return await ReadTemplatesAsync(TemplateIdsOf(template.Members), [.. readSoFar, template], cancellationToken)
            .ConfigureAwait(false);
    }

    private static IEnumerable<TemplateId> TemplateIdsOf(IEnumerable<ListedTag> tags) =>
        tags.Select(tag => tag.TagDefinition.TemplateId).OfType<TemplateId>();

    private static IEnumerable<TemplateId> TemplateIdsOf(IEnumerable<TemplateMember> members) =>
        members.Select(member => member.TagDefinition.TemplateId).OfType<TemplateId>();

    private async Task<TemplateDefinition> ReadTemplateAsync(TemplateId templateId, CancellationToken cancellationToken)
    {
        var templateAddress = new TagAddress(TemplatePrefix + templateId.Value.ToString(CultureInfo.InvariantCulture));
        var template = await ReadRawTagValueAsync(templateAddress, cancellationToken).ConfigureAwait(false);

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

    private async Task<ReadOnlyMemory<byte>> ReadRawTagValueAsync(
        TagAddress schemaTagAddress, CancellationToken cancellationToken)
    {
        using var access = accessFactory.CreateAccessForTagAddress(schemaTagAddress);
        var readResult = await access.ReadAsync(cancellationToken).ConfigureAwait(false);

        if (!readResult.Succeeded)
        {
            throw new DataRetrievalException(
                $"Could not browse the controller symbol table via '{schemaTagAddress}': {readResult.Error}");
        }

        return readResult.Buffer;
    }
}
