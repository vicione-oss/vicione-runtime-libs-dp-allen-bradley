using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;

/// <summary>
/// One data point and everything known about it: the configured <see cref="DataPoint"/>, the controller's
/// <see cref="Metadata"/> for its tag, and the read/write handle. This is the single object every consumer
/// passes around — the batches, decode and verification all project off it — the way S7's
/// <c>ISymbolicDataPointAccess</c> is the source of truth for its data point.
/// </summary>
internal interface ILogixTag : IDisposable
{
    /// <summary>The configured data point this tag reads and writes.</summary>
    ILogixDataPoint DataPoint { get; init; }

    /// <summary>
    /// What the controller's symbol table reports for the tag, or <c>null</c> when the tag is absent from
    /// it — the "not on the controller" signal verification reports.
    /// </summary>
    TagDefinition? Metadata { get; init; }

    /// <summary>
    /// The configured-against-reported pair configuration verification takes. Defaulted rather than
    /// implemented per tag because it is a projection of the two getters above and nothing else, and both
    /// are immutable — so it is the same pair every time it is asked for.
    /// </summary>
    ResolvedDataPoint Resolved => new(DataPoint, Metadata);

    /// <summary>Reads the tag from the controller and returns its raw bytes.</summary>
    Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken);

    /// <summary>Writes <paramref name="buffer"/> to the tag on the controller.</summary>
    Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken);
}
