using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer.Mapping;

public class ArrayContainerNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public ArrayContainerNodePropertyValidator()
    {
        Include(new TagNamePropertyValidator());
    }
}
