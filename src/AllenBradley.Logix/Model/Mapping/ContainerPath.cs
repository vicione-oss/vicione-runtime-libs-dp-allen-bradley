using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;

/// <summary>
/// The containers the tree walk has passed through on its way down to a data point node, each as the
/// segment it contributes to the address, in the order they were passed. Each node is appended as
/// what it is; which name is the tag and which are members is decided only when the path is read back
/// into a <see cref="TagPath"/>. That is where a shape the port has no address for is refused.
/// </summary>
internal sealed record ContainerPath(IReadOnlyList<ContainerPathSegment> Segments)
{
    /// <summary>The path at the device, before any scope: nothing passed through yet.</summary>
    internal static readonly ContainerPath Root = new([]);

    /// <summary>
    /// This path with <paramref name="container"/> passed through next: its program for program scope,
    /// its tag name for an array or UDT container, and nothing for controller scope.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="container"/> is no container the walk knows.</exception>
    internal ContainerPath Append(ILogixContainerNode container) => container switch
    {
        ControllerTagsNode => this,
        ProgramTagsNode program => Append(new ContainerPathSegment.Program(program.ProgramName)),
        ArrayContainerNode array => Append(new ContainerPathSegment.ArrayContainer(array.TagName)),
        UdtContainerNode udt => Append(new ContainerPathSegment.Udt(udt.TagName)),
        _ => throw new NotSupportedException($"'{container.GetType().Name}' is no container the walk knows.")
    };

    /// <summary>
    /// This path ending at <paramref name="dataPoint"/>: the subscript its name is under an array
    /// container, its name anywhere else.
    /// </summary>
    internal ContainerPath Append(ILogixDataPointNode dataPoint, ILogixContainerNode container) => container switch
    {
        ArrayContainerNode => Append(new ContainerPathSegment.ArrayElement(dataPoint.TagName.ToElementIndex())),
        _ => Append(new ContainerPathSegment.DataPoint(dataPoint.TagName))
    };

    /// <summary>This path with <paramref name="containerPathSegment"/> passed through next.</summary>
    internal ContainerPath Append(ContainerPathSegment containerPathSegment) =>
        new([.. Segments, containerPathSegment]);

    /// <summary>
    /// The path as the four parts a tag address is composed from. The first named segment is the tag,
    /// every named segment behind it is a member, and a subscript ends the path.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The path has no tag, names a program behind the tag, or reaches on behind a subscript —
    /// <c>Motors[2].Speed</c> is not a path this port has.
    /// </exception>
    internal TagPath ToTagPath()
    {
        ProgramName? program = null;
        TagName? tag = null;
        UdtMemberPath? members = null;
        ElementIndex? element = null;

        foreach (var segment in Segments)
        {
            switch (segment)
            {
                case ContainerPathSegment.Program scope when program is null && tag is null:
                    program = scope.ProgramName;
                    break;
                case ContainerPathSegment.Named named when tag is null:
                    tag = named.TagName;
                    break;
                case ContainerPathSegment.Named named when element is null:
                    members = members is { } above
                        ? above.Append(named.TagName.ToMemberName())
                        : UdtMemberPath.Of(named.TagName.ToMemberName());
                    break;
                case ContainerPathSegment.ArrayElement subscript when tag is not null && element is null:
                    element = subscript.Index;
                    break;
                default:
                    throw new InvalidOperationException($"No tag address reaches '{segment}' in {this}.");
            }
        }

        return new TagPath(
            program,
            tag ?? throw new InvalidOperationException($"No tag is passed through in {this}."),
            members,
            element);
    }

    public override string ToString() => $"[{string.Join(" > ", Segments)}]";
}
