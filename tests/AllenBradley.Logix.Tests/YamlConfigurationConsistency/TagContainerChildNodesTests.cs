using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ControllerTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes;
using ViciOne.Suite.DataPort.Extensions.Model.TypedNodes.Mapping;
using ViciOne.Tree.Builder.Rules;
using ViciOne.Tree.Builder.Rules.Yaml;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LinkedNodesDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.NodePropertyFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.YamlConfigurationConsistency;

/// <summary>
/// Neither side of the comparison is written out here: the offered set is read from the manifest and the
/// admitted set is what the mapped scope node answers for the node each mapper produces.
/// </summary>
public sealed class TagContainerChildNodesTests
{
    private const string AnyTagName = "AnyTag";

    private const string AnyProgramName = "AnyProgram";

    private const uint AnyElementCount = 10;

    // Read as the engine reads it, so the anchors the file shares between the two scopes arrive expanded.
    private static readonly Ruleset Ruleset =
        RulesDeserializer.Deserialize(LogixYamlConsistencyTests.YamlFileName);

    private static readonly TypedNodeMapper<LogixCommunication, DeviceNode> Mapper =
        TypedLogixNodeMapper.Instance();

    [Theory]
    [InlineData(LogixGeneration.Logix5X70)]
    [InlineData(LogixGeneration.Logix5X80)]
    public void ControllerScopeOffersExactlyTheTypesItsGenerationHas(LogixGeneration generation)
    {
        // Arrange
        var scopeNodeTypeId = ControllerTagsNode.LinkedNodeTypeIdFor(generation);

        // Act
        var offeredTypeIds = TypesOfferedBy(scopeNodeTypeId);

        // Assert
        offeredTypeIds.Should().BeEquivalentTo(TypesAdmittedBy(ScopeNodeOfType(scopeNodeTypeId)));
    }

    [Theory]
    [InlineData(LogixGeneration.Logix5X70)]
    [InlineData(LogixGeneration.Logix5X80)]
    public void ProgramScopeOffersExactlyTheTypesItsGenerationHas(LogixGeneration generation)
    {
        // Arrange
        var scopeNodeTypeId = ProgramTagsNode.LinkedNodeTypeIdFor(generation);

        // Act
        var offeredTypeIds = TypesOfferedBy(scopeNodeTypeId);

        // Assert
        offeredTypeIds.Should().BeEquivalentTo(TypesAdmittedBy(ScopeNodeOfType(scopeNodeTypeId)));
    }

    private static IEnumerable<string> TypesOfferedBy(string scopeNodeTypeId) =>
        Ruleset.NodeTypes
            .Single(nodeType => nodeType.Id == scopeNodeTypeId)
            .ChildNodes
            .Select(child => child.Id);

    private static IEnumerable<string> TypesAdmittedBy(IConfigurationNode scope) =>
        DataPointTypesAdmittedBy(scope).Concat(ContainerTypesAdmittedBy(scope));

    private static IEnumerable<string> DataPointTypesAdmittedBy(IConfigurationNode scope) =>
        Mapper.DataPointNodeMappers
            .Where(mapper => scope.CanBeAdded(mapper.Map(AnyNodeOfType(mapper.TargetLinkedNodeTypeId))))
            .Select(mapper => mapper.TargetLinkedNodeTypeId);

    private static IEnumerable<string> ContainerTypesAdmittedBy(IConfigurationNode scope) =>
        Mapper.ConfigurationNodeMappers
            .Where(mapper => scope.CanBeAdded(mapper.Map(AnyNodeOfType(mapper.TargetLinkedNodeTypeId))))
            .Select(mapper => mapper.TargetLinkedNodeTypeId);

    private static IConfigurationNode ScopeNodeOfType(string scopeNodeTypeId) =>
        Mapper.ConfigurationNodeMappers
            .Single(mapper => mapper.TargetLinkedNodeTypeId == scopeNodeTypeId)
            .Map(AnyNodeOfType(scopeNodeTypeId));

    // A mapper reads only the properties its type needs, so one node carrying all of them serves all.
    private static LinkedNode AnyNodeOfType(string linkedNodeTypeId) =>
        CreateLinkedNode(
            linkedNodeTypeId,
            AnyTagName,
            CreateTagName(AnyTagName),
            CreatePollFrequency(DefaultPollFrequencyInMilliseconds),
            CreateMaxLength(StringMaxLength.Standard.Value),
            CreateElementCount(AnyElementCount),
            CreateProgramName(AnyProgramName));
}
