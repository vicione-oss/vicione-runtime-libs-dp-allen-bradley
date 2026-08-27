using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;

/// <summary>
/// A configured data point that failed verification, with every way it disagrees with the controller.
/// This is the client's own result type: the walking-skeleton slice adapts it onto the DataPort
/// framework's verification result, so this layer carries no dependency on the Extensions seam.
/// </summary>
/// <param name="DataPoint">The data point whose configuration is wrong.</param>
/// <param name="Mismatches">Every mismatch found for it; never empty.</param>
internal readonly record struct MisconfiguredLogixDataPoint(
    ILogixDataPoint DataPoint,
    IReadOnlyList<LogixConfigurationMismatch> Mismatches);
