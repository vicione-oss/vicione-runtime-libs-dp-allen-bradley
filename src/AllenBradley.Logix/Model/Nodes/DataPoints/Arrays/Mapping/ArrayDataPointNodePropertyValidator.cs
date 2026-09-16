using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Arrays.Mapping;

/// <summary>
/// The rules every tag node has plus the one only an array has: a declared count that is a positive
/// number of elements.
/// </summary>
internal sealed class ArrayDataPointNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public ArrayDataPointNodePropertyValidator()
    {
        Include(new DataPointNodePropertyValidator());
        Include(new ArrayNodePropertyValidator());
    }
}
