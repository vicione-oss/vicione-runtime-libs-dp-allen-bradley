using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;

/// <summary>
/// One data point and everything known about it: the configured <see cref="DataPoint"/>, the controller's
/// <see cref="Metadata"/> for its tag, and the read/write handle. This is the single object every consumer
/// passes around — the batches, decode and verification all project off it — the way S7's
/// <c>ISymbolicDataPointAccess</c> is the source of truth for its data point.
/// </summary>
/// <remarks>
/// It wraps the minimal <see cref="ILogixTagAccess"/> seam and adds two immutable getters, so the
/// whole-exchange concurrency contract of ADR-001 is untouched: <see cref="Metadata"/> and
/// <see cref="DataPoint"/> are data, not handle state. Disposal frees the inner handle.
/// </remarks>
internal interface ILogixTag : IDisposable
{
    /// <summary>The configured data point this tag reads and writes.</summary>
    ILogixDataPoint DataPoint { get; }

    /// <summary>
    /// What the controller's symbol table reports for the tag, or <c>null</c> when the tag is absent from
    /// it — the "not on the controller" signal verification reports, and the type-code gate decode checks.
    /// </summary>
    TagDefinition? Metadata { get; }

    /// <summary>Reads the tag from the controller and returns its raw bytes.</summary>
    Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken);

    /// <summary>Writes <paramref name="buffer"/> to the tag on the controller.</summary>
    Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken);
}
