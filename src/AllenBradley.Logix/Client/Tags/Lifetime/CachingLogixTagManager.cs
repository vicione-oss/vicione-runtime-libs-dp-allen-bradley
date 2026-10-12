using Microsoft.Extensions.Logging;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Symbols;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// Owns the controller's symbol table and hands out one <see cref="ILogixTag"/> per data point, kept in a
/// <see cref="TagCache{TDataPoint, TTag}"/> for this manager's lifetime. Data points are records, so the
/// whole data point is the key: poll frequency is part of it, and one tag configured at two frequencies
/// is two points and two tags.
/// </summary>
internal sealed class CachingLogixTagManager(
    ILogixTagAccessFactory factory,
    ISymbolTableLoader symbolTableLoader,
    ILogger<CachingLogixTagManager> logger)
    : ILogixTagManager
{
    private readonly TagCache<ILogixDataPoint, ILogixTag> _tags = new(logger);
    private readonly Lock _gate = new();

    // A sync Lock cannot be held across the browse, so the wait for it needs its own async gate. Never
    // disposed, deliberately: it owns no unmanaged resource, and disposing it would let a client disposed
    // mid-browse throw out of the release below's finally.
    private readonly SemaphoreSlim _loadGate = new(1, 1);

    private SymbolTable? _symbolTable;
    private bool _disposed;

    /// <inheritdoc />
    public async Task LoadSymbolTableAsync(CancellationToken cancellationToken)
    {
        // Checking the field and then browsing is a check-then-act, so the gate is what makes a
        // concurrent second call a no-op rather than a second round trip. A gate and not a cached
        // Lazy<Task>: a browse that failed must not be the answer every later connect gets.
        await _loadGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (_symbolTable is not null)
                {
                    return;
                }
            }

            var symbolTable = await symbolTableLoader.LoadAsync(cancellationToken).ConfigureAwait(false);

            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _symbolTable = symbolTable;
            }
        }
        finally
        {
            _loadGate.Release();
        }
    }

    /// <inheritdoc />
    public ILogixTag TagFor(ILogixDataPoint dataPoint)
    {
        // Held across the cache lookup, so a drain cannot drop the symbol table between the check below
        // and the tag that is created from it.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_symbolTable is not { } symbolTable)
            {
                throw new InvalidOperationException(
                    "The symbol table must be loaded before a tag is requested; " +
                    "connect calls LoadSymbolTableAsync before the first TagFor.");
            }

            return _tags.GetOrAdd(dataPoint, point => new LogixTag(
                point, symbolTable.GetDeclaredTypeAtPath(point.TagPath), factory.CreateAccessForDatapoint(point)));
        }
    }

    /// <inheritdoc />
    public void Drain()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _tags.Drain();
            _symbolTable = null;
        }
    }

    /// <summary>Drains the manager and ends its life, freeing every libplctag handle it created.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _tags.Dispose();
            _symbolTable = null;
        }
    }
}
