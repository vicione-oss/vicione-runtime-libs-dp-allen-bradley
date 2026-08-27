namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// The size in bytes of a single element, as the controller's <c>@tags</c> listing reports it.
/// </summary>
/// <param name="Value">The per-element size in bytes.</param>
public readonly record struct ElementLength(int Value);
