namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Schema;

/// <summary>
/// Reads the controller's symbol table once and decodes it into a <see cref="LogixControllerSchema"/>.
/// This is the metadata source for configuration verification — the libplctag equivalent of loading a
/// symbol tree at connect, browsed value-free through <c>@tags</c> rather than any tag's data.
/// </summary>
internal interface ILogixSchemaBrowser
{
    /// <summary>Browses and decodes the controller's tag directory.</summary>
    /// <exception cref="LogixSchemaException">The symbol table could not be read.</exception>
    Task<LogixControllerSchema> BrowseAsync(CancellationToken cancellationToken);
}
