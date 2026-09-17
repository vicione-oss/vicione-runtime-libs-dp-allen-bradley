using FluentValidation;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Mapping;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer.Mapping;

/// <summary>
/// The tag-name rule alone. A member name obeys the same rule as a tag name, so a nested UDT
/// passes the same gate; no poll frequency, because a container is not polled itself.
/// </summary>
public sealed class UdtContainerNodePropertyValidator : AbstractValidator<LinkedNode>
{
    public UdtContainerNodePropertyValidator()
    {
        Include(new TagNamePropertyValidator());
    }
}
