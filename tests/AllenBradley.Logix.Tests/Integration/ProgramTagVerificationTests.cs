using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Mapper;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Verification;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Integration;

/// <summary>
/// The join between the two halves of program scope, against the real CompactLogix L32E: the address the
/// configuration tree composes, and the key the symbol-table browse files a program tag under. Both are
/// <c>Program:MainProgram.strValue1</c> by construction, and this is what holds them to it.
/// </summary>
/// <remarks>
/// The whole chain is the shipping code — <see cref="TypedLogixNodeMapper"/> to
/// <see cref="LogixDataPointsGroupsMapper"/> to <see cref="LogixConfigurationVerifier"/> over the
/// connected client — because a prefix that agreed with itself and not with the controller would pass
/// every unit test in the suite. Requires the device reachable (see TEST-DEVICE-SETUP.md).
/// </remarks>
public class ProgramTagVerificationTests : LogixIntegrationTestBase
{
    [Fact]
    public async Task Verify_AStringTagConfiguredUnderItsProgram_ReportsNoMisconfiguration()
    {
        // Arrange
        // strValue1 is program-scoped on the L32E — configured bare, under the program that owns it.
        var communication = CreateCommunicationOf(
            [
                CreateProgramTagsNode("MainProgram", ProgramTagsId),
                CreateStringNode("Label", "strValue1", maxLength: 82, parentId: ProgramTagsId),
            ],
            DeviceNode.CompactLogix5x70DesignId);

        var deviceNode = TypedLogixNodeMapper.Instance().MapToTypedNodes(communication);
        var dataPoints = new LogixDataPointsGroupsMapper().ToDataPoints(deviceNode);
        var verifier = new LogixConfigurationVerifier(Client);

        // Act
        var misconfigured = await verifier.Verify(dataPoints, TestContext.Current.CancellationToken);

        // Assert
        dataPoints.Should().ContainSingle()
            .Which.TagName.Value.Should().Be(LogixTagAddresses.StrValue1);
        misconfigured.Should().BeEmpty();
    }
}
