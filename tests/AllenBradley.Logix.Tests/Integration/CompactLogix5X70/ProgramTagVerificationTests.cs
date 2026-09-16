using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.ProgramTagsNodeTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.TagNodeTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration.CompactLogix5X70;

/// <summary>
/// The join between the two halves of program scope, against the real CompactLogix L32E: the address the
/// configuration tree composes, and the key the symbol-table browse files a program tag under.
/// </summary>
public sealed class ProgramTagVerificationTests : LogixIntegrationTestBase
{
    [Fact]
    public async Task AStringTagConfiguredUnderTheProgramThatOwnsItReportsNoMisconfiguration()
    {
        // Arrange
        // strValue1 is program-scoped on the L32E — configured bare, under the program that owns it.
        var mainProgram = CreateProgramTagsNode("MainProgram");
        var communication = CreateCommunicationOf(
            [
                mainProgram,
                CreateStringNode("Label", "strValue1", mainProgram.Id, maxLength: 82),
            ],
            DeviceNode.CompactLogix5X70DesignId);

        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);
        var dataPoints = new LogixDataPointsGroupsMapper().ToDataPoints(deviceNode);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var misconfigured = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        dataPoints.Should().ContainSingle()
            .Which.TagAddress.Value.Should().Be(BenchControllerTags.StrValue1);
        misconfigured.Should().BeEmpty();
    }
}
