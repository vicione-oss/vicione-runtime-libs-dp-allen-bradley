using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;

/// <summary>
/// Reads the controller's symbols once, through the <c>@tags</c> pseudo-address rather than any tag's data, and decodes
/// them into a <see cref="SymbolTable"/> for configuration verification to diff against.
/// </summary>
internal interface ISymbolTableLoader
{
    /// <summary>Browses and decodes the controller's tag directory.</summary>
    /// <exception cref="DataRetrievalException">The symbol table could not be read.</exception>
    Task<SymbolTable> LoadAsync(CancellationToken cancellationToken);
}
