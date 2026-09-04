using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// Owns the controller's symbol table and resolves the <see cref="ILogixTag"/> for a data
/// point — data point, metadata and handle joined into one object.
/// <see cref="CachingLogixTagManager"/> is the production implementation; tests supply a fake that
/// hands back in-process tags.
/// A returned tag is borrowed, never owned: the manager disposes it, and a consumer that disposes one breaks
/// every other holder. <see cref="Drain"/> frees the connect-scoped state and leaves the manager ready to
/// load again; <see cref="IDisposable.Dispose"/> is terminal.
/// </summary>
internal interface ILogixTagManager : IDisposable
{
    /// <summary>
    /// Browses the controller's symbol table once and retains it, so <see cref="TagFor"/> can stamp each
    /// tag with the controller's metadata. Idempotent — a second call is a no-op. Connect calls this before
    /// the first <see cref="TagFor"/>.
    /// Concurrent callers share one browse. A browse that fails is not remembered, so the next call tries
    /// again.
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

    /// <summary>
    /// Disposes every tag this manager created and drops the schema, putting the manager back to its
    /// pre-connect state. Idempotent, and reversible: a later <see cref="LoadTagDefinitionsAsync"/> browses again
    /// and <see cref="TagFor"/> recreates the tags it needs.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The manager has been disposed.</exception>
    void Drain();
}
