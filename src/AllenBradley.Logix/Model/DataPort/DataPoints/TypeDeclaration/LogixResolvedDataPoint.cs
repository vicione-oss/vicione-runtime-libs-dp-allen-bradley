namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// Pairs a configured data point with the metadata the controller reports for its tag, so verification
/// can compare expected against actual. <see cref="Device"/> is <c>null</c> when the tag is not on the
/// controller at all — an absent tag is itself a misconfiguration.
/// </summary>
/// <param name="DataPoint">The configured data point.</param>
/// <param name="Device">The controller's declaration for the tag, or <c>null</c> when it was not found.</param>
public readonly record struct LogixResolvedDataPoint(
    ILogixDataPoint DataPoint,
    LogixTypeDeclaration? Device);
