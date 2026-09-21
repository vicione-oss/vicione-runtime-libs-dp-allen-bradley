using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// Owns the controller's symbol table and resolves the <see cref="ILogixTag"/> for a data point. A
/// returned tag is borrowed, never owned: the manager disposes it, and a consumer that disposes one
/// breaks every other holder. <see cref="Drain"/> is reversible; <see cref="IDisposable.Dispose"/> is
/// terminal.
/// </summary>
internal interface ILogixTagManager : IDisposable
{
    /// <summary>
    /// Browses the controller's symbol table once and retains it, so <see cref="TagFor"/> can stamp each
    /// tag with the controller's metadata. Idempotent, and concurrent callers share one browse; a browse
    /// that fails is not remembered, so the next call tries again.
    /// </summary>
    /// <exception cref="DataRetrievalException">The symbol table could not be browsed.</exception>
    Task LoadSymbolTableAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns the tag for <paramref name="dataPoint"/>, creating or reusing it.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The symbol table has not been loaded yet — <see cref="LoadSymbolTableAsync"/> is a connect precondition.
    /// </exception>
    ILogixTag TagFor(ILogixDataPoint dataPoint);

    /// <summary>
    /// Disposes every tag this manager created and drops the symbol table, putting it back to its pre-connect
    /// state. A later <see cref="LoadSymbolTableAsync"/> browses again and <see cref="TagFor"/>
    /// recreates the tags it needs.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The manager has been disposed.</exception>
    void Drain();
}
