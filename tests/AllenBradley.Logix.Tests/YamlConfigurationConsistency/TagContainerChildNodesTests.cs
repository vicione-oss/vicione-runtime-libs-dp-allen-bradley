using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.TreeBuilder.Rules;
using ViciOne.TreeBuilder.Rules.Yaml;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.YamlConfigurationConsistency;

/// <summary>
/// Neither side of the comparison is written out here: the offered set is read from the manifest and the
/// admitted set from the node each data-point mapper produces.
/// </summary>
public sealed class TagContainerChildNodesTests
{
    private const string AnyTagName = "AnyTag";

    private const uint AnyElementCount = 10;

    // Read as the engine reads it, so the anchors the file shares between the two scopes arrive expanded.
    private static readonly Ruleset Ruleset =
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

    private static IEnumerable<string> TypesOfferedBy(string containerNodeTypeId) =>
        Ruleset.NodeTypes
            .Single(nodeType => nodeType.Id == containerNodeTypeId)
            .ChildNodes
            .Select(child => child.Id);

    // Mirrors the comparison ITagScopeNode.CanHold makes.
    private static IEnumerable<string> TypesAdmittedOn(LogixGeneration generation) =>
        MinimumGenerationsByNodeTypeId()
            .Where(type => type.Value <= generation)
            .Select(type => type.Key);

    private static Dictionary<string, LogixGeneration> MinimumGenerationsByNodeTypeId() =>
        TypedLogixNodeMapper.Instance().DataPointNodeMappers.ToDictionary(
            mapper => mapper.TargetLinkedNodeTypeId,
            mapper => ((ILogixDataPointNode)mapper.Map(AnyTagNodeOfType(mapper.TargetLinkedNodeTypeId)))
                .MinimumGeneration);

    // A mapper reads only the properties its type needs, so one node carrying all of them serves all.
    private static LinkedNode AnyTagNodeOfType(string linkedNodeTypeId) =>
        CreateLinkedNode(
            linkedNodeTypeId,
            AnyTagName,
            CreateTagName(AnyTagName),
            CreatePollFrequency(DefaultPollFrequencyInMilliseconds),
            CreateMaxLength(StringMaxLength.Standard.Value),
            CreateElementCount(AnyElementCount));
}
