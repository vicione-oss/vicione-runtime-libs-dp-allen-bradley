using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Definitions;

/// <summary>
/// Reads the controller's symbols once and decodes it into a <see cref="TagDefinitions"/>.
/// This is the metadata source for configuration verification — the libplctag equivalent of loading a
/// symbol tree at connect, browsed value-free through <c>@tags</c> rather than any tag's data.
/// </summary>
internal interface ITagDefinitionsLoader
{
    /// <summary>Browses and decodes the controller's tag directory.</summary>
    /// <exception cref="DataRetrievalException">The symbol table could not be read.</exception>
    Task<TagDefinitions> LoadAsync(CancellationToken cancellationToken);
}
