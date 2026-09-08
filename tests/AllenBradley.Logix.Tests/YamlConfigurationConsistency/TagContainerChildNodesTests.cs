using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints.Scalars;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData;
using ViciOne.TreeBuilder.Rules.Yaml;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.YamlConfigurationConsistency;

/// <summary>
/// Holds each tag container's child list against <see cref="ILogixScalarNode.MinimumGeneration"/>: a
/// container offers exactly the types its generation has, no more and no fewer.
/// </summary>
/// <remarks>
/// The child lists are the editor's half of the generation rule, and until now nothing checked them —
/// <c>ITagScopeNode.CanHold</c> is the guard for a configuration the editor did not build, and it never
/// runs for one the editor never offered. Dropping <c>ULInt</c> from the 5X80 group would have made the
/// type unreachable with every unit test still green.
/// <para>
/// Neither side is written out here. The offered set is read from the manifest, the admitted set from
/// the node each data-point mapper produces, so a new type is covered the moment its mapper is
/// assembled — and the test says which of the two sides forgot it rather than that a list changed.
/// </para>
/// </remarks>
public sealed class TagContainerChildNodesTests
{
    // The manifest as the engine reads it, rather than as YAML text: the child lists arrive resolved,
    // so the anchors the file shares between the two scopes are already expanded here.
    private static readonly TreeBuilder.Rules.Ruleset Ruleset =
        RulesDeserializer.Deserialize(LogixYamlConsistencyTests.YamlFileName);

    [Theory]
    [InlineData(LogixGeneration.Logix5X70)]
    [InlineData(LogixGeneration.Logix5X80)]
    public void ControllerScopeOffersExactlyTheTypesItsGenerationHas(LogixGeneration generation)
    {
        // Arrange
        var linkedNodeTypeIdFor = ControllerTagsNode.LinkedNodeTypeIdFor(generation);

        // Act
        var offeredTypeIds = TypesOfferedBy(linkedNodeTypeIdFor);

        // Assert
        offeredTypeIds.Should().BeEquivalentTo(TypesAdmittedOn(generation));
    }

    [Theory]
    [InlineData(LogixGeneration.Logix5X70)]
    [InlineData(LogixGeneration.Logix5X80)]
    public void ProgramScopeOffersExactlyTheTypesItsGenerationHas(LogixGeneration generation)
    {
        // Arrange
        var linkedNodeTypeIdFor = ProgramTagsNode.LinkedNodeTypeIdFor(generation);

        // Act
        var offeredTypeIds = TypesOfferedBy(linkedNodeTypeIdFor);

        // Assert
        offeredTypeIds.Should().BeEquivalentTo(TypesAdmittedOn(generation));
    }

    // What the editor offers under the container: the node ids the manifest lists as its children.
    private static IEnumerable<string> TypesOfferedBy(string containerNodeTypeId) =>
        Ruleset.NodeTypes
            .Single(nodeType => nodeType.Id == containerNodeTypeId)
            .ChildNodes
            .Select(child => child.Id);

    // What the container would take if it were asked: every configured type whose own minimum
    // generation this one meets, which is the comparison ITagScopeNode.CanHold makes.
    private static IEnumerable<string> TypesAdmittedOn(LogixGeneration generation) =>
        MinimumGenerationsByNodeTypeId()
            .Where(type => type.Value <= generation)
            .Select(type => type.Key);

    // Every data-point type the addon assembles a mapper for, paired with the generation its node
    // declares. One linked node serves all of them: a mapper reads the properties its own type needs
    // and ignores the rest, so carrying all three satisfies the widest of them (a STRING's MaxLength)
    // without the narrower ones noticing.
    private static Dictionary<string, LogixGeneration> MinimumGenerationsByNodeTypeId()
    {
        var mappers = TypedLogixNodeMapper.Instance().DataPointNodeMappers;

        return mappers.ToDictionary(
            mapper => mapper.TargetLinkedNodeTypeId,
            mapper => ((ILogixScalarNode)mapper.Map(AnyScalarNodeOfType(mapper.TargetLinkedNodeTypeId)))
                .MinimumGeneration);
    }

    private static LinkedNode AnyScalarNodeOfType(string linkedNodeTypeId) =>
        CreateLinkedNode(
            linkedNodeTypeId,
            "AnyTag",
            NodePropertyFactory.CreateTagName("AnyTag"),
            NodePropertyFactory.CreatePollFrequency(100),
            NodePropertyFactory.CreateMaxLength(82));
}
