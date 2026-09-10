using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// Reads the controller's symbols once, through <c>@tags</c> rather than any tag's data, and decodes
/// them into a <see cref="TagDefinitions"/> for configuration verification to diff against.
/// </summary>
internal interface ITagDefinitionsLoader
{
    /// <summary>Browses and decodes the controller's tag directory.</summary>
    /// <exception cref="DataRetrievalException">The symbol table could not be read.</exception>
    Task<TagDefinitions> LoadAsync(CancellationToken cancellationToken);
}
