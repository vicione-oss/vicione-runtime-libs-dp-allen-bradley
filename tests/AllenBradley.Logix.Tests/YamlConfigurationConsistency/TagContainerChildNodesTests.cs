using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.TreeBuilder.Rules.Yaml;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.YamlConfigurationConsistency;

/// <summary>
/// Neither side of the comparison is written out here: the offered set is read from the manifest and the
/// admitted set from the node each data-point mapper produces, so a new type is covered the moment its
/// mapper is assembled.
/// </summary>
public sealed class TagContainerChildNodesTests
{
    private const string AnyTagName = "AnyTag";

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
        var containerNodeTypeId = ControllerTagsNode.LinkedNodeTypeIdFor(generation);

        // Act
        var offeredTypeIds = TypesOfferedBy(containerNodeTypeId);

        // Assert
        offeredTypeIds.Should().BeEquivalentTo(TypesAdmittedOn(generation));
    }

    [Theory]
    [InlineData(LogixGeneration.Logix5X70)]
    [InlineData(LogixGeneration.Logix5X80)]
    public void ProgramScopeOffersExactlyTheTypesItsGenerationHas(LogixGeneration generation)
    {
        // Arrange
        var containerNodeTypeId = ProgramTagsNode.LinkedNodeTypeIdFor(generation);

        // Act
        var offeredTypeIds = TypesOfferedBy(containerNodeTypeId);

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
    // declares.
    private static Dictionary<string, LogixGeneration> MinimumGenerationsByNodeTypeId() =>
        TypedLogixNodeMapper.Instance().DataPointNodeMappers.ToDictionary(
            mapper => mapper.TargetLinkedNodeTypeId,
            mapper => ((ILogixTagNode)mapper.Map(AnyTagNodeOfType(mapper.TargetLinkedNodeTypeId)))
                .MinimumGeneration);

    // One linked node serves every mapper: a mapper reads the properties its own type needs and ignores
    // the rest, so carrying all three satisfies the widest of them (a STRING's MaxLength).
    private static LinkedNode AnyTagNodeOfType(string linkedNodeTypeId) =>
        CreateLinkedNode(
            linkedNodeTypeId,
            AnyTagName,
            CreateTagName(AnyTagName),
            CreatePollFrequency(DefaultPollFrequencyInMilliseconds),
            CreateMaxLength(StringMaxLength.Standard.Value));
}
