using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;

/// <summary>
/// A tag read as one whole array: the shape, with nothing of the element type. It is what a caller
/// holding an <see cref="ILogixDataPoint"/> asks when the answer turns on how many elements the tag holds
/// and not on what they are — the width of the libplctag handle is the one that does today.
/// </summary>
public interface ILogixArrayDataPoint : ILogixDataPoint
{
    /// <summary>How many elements the tag is declared with in Studio 5000.</summary>
    ElementCount ElementCount { get; }
}
