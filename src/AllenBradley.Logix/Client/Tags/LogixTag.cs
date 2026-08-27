using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;

/// <summary>
/// The production <see cref="ILogixTag"/>: it joins the data point and the controller's
/// metadata onto the access the factory built, and delegates every read and write to that inner
/// <see cref="ILogixTagAccess"/> (still a <see cref="SynchronizedLogixTagAccess"/>, so ADR-001's
/// whole-exchange gating is preserved). Disposing this disposes the inner handle exactly once.
/// </summary>
/// <param name="dataPoint">The configured data point.</param>
/// <param name="metadata">The controller's declaration for the tag, or <c>null</c> when it is absent.</param>
/// <param name="inner">The access this tag reads and writes over.</param>
internal sealed class LogixTag(
    ILogixDataPoint dataPoint, LogixTypeDeclaration? metadata, ILogixTagAccess inner)
    : ILogixTag
{
    /// <inheritdoc />
    public ILogixDataPoint DataPoint => dataPoint;

    /// <inheritdoc />
    public LogixTypeDeclaration? Metadata => metadata;

    /// <inheritdoc />
    public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken) =>
        inner.ReadAsync(cancellationToken);

    /// <inheritdoc />
    public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
        inner.WriteAsync(buffer, cancellationToken);

    /// <summary>Frees the inner handle — the only owned resource; the data point and metadata are data.</summary>
    public void Dispose() => inner.Dispose();
}
