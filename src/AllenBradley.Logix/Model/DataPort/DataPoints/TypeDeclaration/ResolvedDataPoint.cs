namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// Pairs a configured data point with what the controller declares for its path, so verification can
/// compare expected against actual.
/// </summary>
/// <param name="DataPoint">The configured data point.</param>
/// <param name="DeclaredType">
/// What the controller declares the path to be, or <c>null</c> when the path is not on the controller
/// at all.
/// </param>
public readonly record struct ResolvedDataPoint(
    ILogixDataPoint DataPoint,
    DeclaredType? DeclaredType);
