namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// Pairs a configured data point with what the controller reports for its tag, so verification can
/// compare expected against actual.
/// </summary>
/// <param name="DataPoint">The configured data point.</param>
/// <param name="TagDefinition">
/// The controller's declaration for the tag, or <c>null</c> when the tag is not on the controller at all.
/// </param>
public readonly record struct ResolvedDataPoint(
    ILogixDataPoint DataPoint,
    TagDefinition? TagDefinition);
