using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// The production <see cref="ILogixTag"/>: it joins the data point and the controller's
/// metadata onto the access the factory built, and delegates every read and write to that inner
/// <see cref="ILogixTagAccess"/> (still a <see cref="SynchronizedLogixTagAccess"/>, so the
/// one-operation-at-a-time gating of <c>ADR/2026-07-16-operations-not-accessors-over-libplctag.md</c> is preserved).
/// Disposing this disposes the inner handle exactly once.
/// </summary>
internal sealed record LogixTag(ILogixDataPoint DataPoint, TagDefinition? Metadata, ILogixTagAccess Access)
    : ILogixTag
{

    /// <inheritdoc />
    public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken) =>
        Access.ReadAsync(cancellationToken);

    /// <inheritdoc />
    public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
        Access.WriteAsync(buffer, cancellationToken);

    /// <summary>Frees the inner handle — the only owned resource; the data point and metadata are data.</summary>
    public void Dispose() => Access.Dispose();
}
