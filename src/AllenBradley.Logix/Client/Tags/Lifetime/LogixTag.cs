using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Lifetime;

/// <summary>
/// The production <see cref="ILogixTag"/>: the data point and the controller's metadata joined onto the
/// access the factory built, with every read and write delegated to that access — still a
/// <see cref="SynchronizedLogixTagAccess"/>, so its gating is preserved.
/// </summary>
internal sealed record LogixTag(ILogixDataPoint DataPoint, DeclaredType? DeclaredType, ILogixTagAccess Access)
    : ILogixTag
{
    /// <inheritdoc />
    public Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken) =>
        Access.ReadAsync(cancellationToken);

    /// <inheritdoc />
    public Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken) =>
        Access.WriteAsync(buffer, cancellationToken);

    /// <summary>Frees the inner handle, the only owned resource.</summary>
    public void Dispose() => Access.Dispose();
}
