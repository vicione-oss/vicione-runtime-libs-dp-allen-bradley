using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.ArrayContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.Scope.ProgramTags;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Containers.UdtContainer;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Infrastructure;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static System.Guid;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.ControllerTagsNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.ProgramTagsNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X80.Addressing;

/// <summary>
/// The configuration as the editor builds it (a scope, a UDT Instance, a nested one, and an array container
/// on a member), mapped to data points the way the dataport does it. The addresses the tree composes are
/// checked against the names the controller declares, not typed in.
/// </summary>
public abstract class ConfigurationTreeIntegrationTests(
    TestControllerFixture fixture,
    ITestOutputHelper output,
    TagScope scope)
    : CompactLogix5X80IntegrationTestBase(fixture, output, scope)
{
    private const string Channel = "Value";

    [Fact]
    public async Task ContainerNodesBuildValidTagAddresses()
    {
        // Arrange
        var scopeNode = ScopeNode();
        var udt = CreateContainerNode(UdtContainerNode.Logix5X80LinkedNodeTypeId, Tags.UdtTagName, scopeNode.Id);
        var innerUdt = CreateContainerNode(UdtContainerNode.Logix5X80LinkedNodeTypeId, "InnerUdt", udt.Id);
        var dintArray = CreateContainerNode(ArrayContainerNode.DIntLinkedNodeTypeId, "DIntArray", udt.Id);
        var communication = CreateCommunicationOf(
            [
                scopeNode,
                udt,
                CreateDIntNode(Channel, "DIntTag", udt.Id),
                innerUdt,
                CreateStringNode(Channel, "StringTag", innerUdt.Id),
                dintArray,
                CreateDIntNode(Channel, "[3]", dintArray.Id)
            ],
            DeviceNode.CompactLogix5X80DesignId);
        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);
        var dataPoints = new LogixDataPointsGroupsMapper().ToDataPoints(deviceNode);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var misconfigured = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        dataPoints.Select(dataPoint => dataPoint.TagAddress.Value).Should().BeEquivalentTo(
            Tags.Udt.DInt, Tags.Udt.Inner.String, TagAddresses.ArrayElement(Tags.Udt.DIntArray, 3));
        misconfigured.Should().BeEmpty();
    }

    private Node ScopeNode() => Tags.Scope switch
    {
        TagScope.Controller => CreateControllerTagsNodeFor(DeviceNode.CompactLogix5X80DesignId),
        TagScope.Program => CreateProgramTagsNode("MainProgram", ProgramTagsNode.Logix5X80LinkedNodeTypeId),
        _ => throw new ArgumentOutOfRangeException(nameof(Tags.Scope), Tags.Scope, null)
    };

    private static Node CreateContainerNode(string designId, string tagName, Guid parentId) =>
        new()
        {
            DesignId = designId,
            Name = tagName,
            Id = NewGuid(),
            ParentId = parentId,
            Properties = new Dictionary<string, Property>
            {
                { ILogixDataPointNode.TagNamePropertyName, new Property { Value = tagName } }
            }
        };

    public sealed class ControllerScope(TestControllerFixture fixture, ITestOutputHelper output)
        : ConfigurationTreeIntegrationTests(fixture, output, TagScope.Controller);

    public sealed class ProgramScope(TestControllerFixture fixture, ITestOutputHelper output)
        : ConfigurationTreeIntegrationTests(fixture, output, TagScope.Program);
}
