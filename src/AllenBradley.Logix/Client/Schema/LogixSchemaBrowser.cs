using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Schema;

/// <summary>
/// Browses the controller's symbol table over the <see cref="ILogixTagAccess"/> seam: it reads the
/// <c>@tags</c> directory, then each program's <c>@tags</c>, decodes them, and assembles a
/// <see cref="LogixControllerSchema"/>. The listing handles are transient — read once and disposed —
/// so nothing above the adapter touches the sealed <c>Tag</c>, and no native handle outlives the browse.
/// </summary>
/// <remarks>
/// libplctag has no load-all call, so the browse is one read per scope: the controller, plus one per
/// program. Program tags are keyed by their qualified name (<c>Program:Main.Count</c>) so they match a
/// configured tag address. Nested programs are not walked in this cut.
/// </remarks>
/// <param name="accessFactory">Creates the transient access for each system-tag read.</param>
internal sealed class LogixSchemaBrowser(ILogixTagAccessFactory accessFactory) : ILogixSchemaBrowser
{
    private static readonly TagName ControllerTagDirectory = new("@tags");
    private const string ProgramPrefix = "Program:";

    /// <inheritdoc />
    public async Task<LogixControllerSchema> BrowseAsync(CancellationToken cancellationToken)
    {
        var declarationsByTagName =
            new Dictionary<TagName, LogixTypeDeclaration>(TagName.CaseInsensitiveComparer);

        var controllerTags = await ReadDirectoryAsync(ControllerTagDirectory, cancellationToken).ConfigureAwait(false);
        AddTags(declarationsByTagName, controllerTags, programScope: null);

        foreach (var program in controllerTags.Where(IsProgram))
        {
            var programTags = await ReadDirectoryAsync(new TagName($"{program.TagName.Value}.@tags"), cancellationToken)
                .ConfigureAwait(false);
            AddTags(declarationsByTagName, programTags, programScope: program.TagName);
        }

        return new LogixControllerSchema(declarationsByTagName);
    }

    private async Task<IReadOnlyList<LogixTypeDeclaration>> ReadDirectoryAsync(
        TagName systemTagName, CancellationToken cancellationToken)
    {
        using var access = accessFactory.CreateForSystemTag(systemTagName);
        var read = await access.ReadAsync(cancellationToken).ConfigureAwait(false);

        if (!read.Succeeded)
        {
            throw new LogixSchemaException(
                $"Could not browse the controller symbol table via '{systemTagName}': {read.Error}");
        }

        return LogixSymbolListingDecoder.Decode(read.Buffer.Span);
    }

    private static bool IsProgram(LogixTypeDeclaration declaration) =>
        declaration.TagName.Value.StartsWith(ProgramPrefix, StringComparison.Ordinal);

    private static void AddTags(
        Dictionary<TagName, LogixTypeDeclaration> declarationsByTagName,
        IReadOnlyList<LogixTypeDeclaration> tags,
        TagName? programScope)
    {
        foreach (var tag in tags)
        {
            if (tag.TagName.Value.Length == 0)
            {
                continue;
            }

            // A program tag is keyed by its qualified name so it matches a configured address; its
            // declaration carries that same qualified name.
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
