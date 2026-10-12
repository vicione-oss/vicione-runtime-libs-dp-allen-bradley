using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.Extensions.Exceptions;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// Owns the controller's symbol table and resolves the <see cref="ILogixTag"/> for a data point. A
/// returned tag is borrowed, never owned: the manager disposes it, and a consumer that disposes one
/// breaks every other holder. <see cref="ITagSource{TDataPoint, TTag}.TagFor"/> throws an
/// <see cref="InvalidOperationException"/> until <see cref="LoadSymbolTableAsync"/> has run, because
/// loading the symbol table is a connect precondition. <see cref="Drain"/> is reversible;
/// <see cref="IDisposable.Dispose"/> is terminal.
/// </summary>
internal interface ILogixTagManager : ITagSource<ILogixDataPoint, ILogixTag>, IDisposable
{
    /// <summary>
    /// Browses the controller's symbol table once and retains it, so
    /// <see cref="ITagSource{TDataPoint, TTag}.TagFor"/> can stamp each tag with the controller's metadata. Idempotent, and concurrent callers share one browse; a browse
    /// that fails is not remembered, so the next call tries again.
    /// </summary>
    /// <exception cref="DataRetrievalException">The symbol table could not be browsed.</exception>
    Task LoadSymbolTableAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Disposes every tag this manager created and drops the symbol table, putting it back to its pre-connect
    /// state. A later <see cref="LoadSymbolTableAsync"/> browses again and <see cref="ITagSource{TDataPoint, TTag}.TagFor"/>
    /// recreates the tags it needs.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The manager has been disposed.</exception>
    void Drain();
}
