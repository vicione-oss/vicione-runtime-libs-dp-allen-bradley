using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// Owns the controller's symbol table and resolves the <see cref="ILogixTag"/> for a data
/// point — data point, metadata and handle joined into one object.
/// <see cref="CachingLogixTagManager"/> is the production implementation; tests supply a fake that
/// hands back in-process tags.
/// </summary>
/// <remarks>
/// The schema is loaded once at connect (<see cref="LoadTagDefinitionsAsync"/>) and joined onto every tag, so
/// <see cref="TagFor"/> is a schema precondition. A tag returned here may be shared with other
/// callers, so it is borrowed, never owned: the manager disposes it, and a consumer that disposes one it
/// did not create breaks every other holder.
/// </remarks>
internal interface ILogixTagManager
{
    /// <summary>
    /// Browses the controller's symbol table once and retains it, so <see cref="TagFor"/> can stamp each
    /// tag with the controller's metadata. Idempotent — a second call is a no-op. Connect calls this before
    /// the first <see cref="TagFor"/>.
    /// </summary>
    /// <exception cref="DataRetrievalException">The symbol table could not be browsed.</exception>
    Task LoadTagDefinitionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns the tag for <paramref name="dataPoint"/>, creating or reusing it, with the controller's
    /// metadata for it joined on.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The schema has not been loaded yet — <see cref="LoadTagDefinitionsAsync"/> is a connect precondition.
    /// </exception>
    ILogixTag TagFor(ILogixDataPoint dataPoint);
}
