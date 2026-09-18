using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;

internal abstract record ContainerPathSegment
{
    /// <summary>A program-scope container: the program its tags are scoped to.</summary>
    internal sealed record Program(ProgramName ProgramName) : ContainerPathSegment;

    /// <summary>A container or node with a declared name: a tag under a scope, a member under a UDT.</summary>
    internal abstract record Named(TagName TagName) : ContainerPathSegment;

    /// <summary>A UDT container, opened into its members.</summary>
    internal sealed record Udt(TagName TagName) : Named(TagName);

    /// <summary>An array container, opened into its elements.</summary>
    internal sealed record ArrayContainer(TagName TagName) : Named(TagName);

    /// <summary>A data point node: the leaf the walk ends at.</summary>
    internal sealed record DataPoint(TagName TagName) : Named(TagName);

    /// <summary>A data point node under an array container: the element its subscript selects.</summary>
    internal sealed record ArrayElement(ElementIndex Index) : ContainerPathSegment;
}
