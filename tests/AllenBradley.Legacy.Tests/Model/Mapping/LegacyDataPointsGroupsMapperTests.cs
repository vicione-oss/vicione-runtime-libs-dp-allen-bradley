using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Mapping;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataFiles.IntegerFile;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.DataPoints.Integers.Integer;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Mapping;

public sealed class LegacyDataPointsGroupsMapperTests
{
    private readonly LegacyDataPointsGroupsMapper _mapper = new();

    [Fact]
    public void AnIntegerElementIsAddressedInTheFileItSitsIn()
    {
        // Arrange
        var file = new IntegerFileNode(LinkedNodeOf(IntegerFileNode.LinkedNodeTypeId), new FileNumber(10));
        var element = new IntegerNode(
            LinkedNodeOf(IntegerNode.LinkedNodeTypeId), new ElementNumber(3), PollFrequency.FromMilliseconds(250));
        file.DataPointNodes.Add(element);
        var device = Device();
        device.ConfigurationNodes.Add(file);

        // Act
        var dataPoints = _mapper.ToDataPoints(device);

        // Assert
        var expected = new IntegerDataPoint(file.FileNumber, element.ElementNumber, element.PollFrequency, element.Channels);
        dataPoints.Should().ContainSingle().Which.Should().Be(expected);
    }

    private static DeviceNode Device() =>
        new(
            new LegacyCommunication { ConnectionEndpoint = "192.168.1.10" },
            new LegacyClientInformation(
                LegacyControllerFamily.Slc500,
                new ConnectionEndpoint("192.168.1.10"),
                TcpPort.EtherNetIp,
                CipRoutePath: null,
                new OperationTimeout(TimeSpan.FromSeconds(5))));

    private static LinkedNode LinkedNodeOf(string designId) =>
        LinkedNodeFactory.Create([new Node { DesignId = designId, Name = designId, Id = Guid.NewGuid() }]).Single();
}
