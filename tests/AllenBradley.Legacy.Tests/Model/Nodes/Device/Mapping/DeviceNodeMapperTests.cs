using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device.LegacyControllerFamily;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Model.Nodes.Device.Mapping;

public sealed class DeviceNodeMapperTests
{
    private readonly DeviceNodeMapper _mapper = new();

    /// <summary>Each device node type with the controller family it stands for.</summary>
    public static TheoryData<string, LegacyControllerFamily> DeviceNodeTypes =>
        new()
        {
            { DeviceNode.Slc500DesignId, Slc500 },
            { DeviceNode.MicroLogixDesignId, MicroLogix },
        };

    [Theory]
    [MemberData(nameof(DeviceNodeTypes))]
    public void ADeviceNodeTypeStandsForTheFamilyItsDesignIdNames(
        string designId, LegacyControllerFamily expectedFamily)
    {
        // Arrange
        var communication = DefaultTestCommunication() with { DesignId = designId };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.Family.Should().Be(expectedFamily);
    }

    [Fact]
    public void ADeviceNodeTypeTheDataPortDoesNotHaveIsRefused()
    {
        // Arrange
        var communication = DefaultTestCommunication() with { DesignId = "DevicePlc5" };

        // Act
        var mapping = _mapper.Invoking(mapper => mapper.CreateRootNode(communication));

        // Assert
        mapping.Should().Throw<InvalidConfigurationException>();
    }

    [Fact]
    public void EveryConnectionPropertyOfAConfigurationReachesTheClientInformation()
    {
        // Arrange
        var communication = DefaultTestCommunication() with
        {
            ConnectionEndpoint = "192.168.1.10",
            TcpPort = 44819,
            CipRoutePath = "1,2,2,3",
            OperationTimeout = 750,
        };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        var expected = new LegacyClientInformation(
            Slc500,
            new ConnectionEndpoint(communication.ConnectionEndpoint),
            new TcpPort(communication.TcpPort),
            new CipRoutePath(communication.CipRoutePath),
            new OperationTimeout(TimeSpan.FromMilliseconds(communication.OperationTimeout)));
        clientInformation.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AControllerThatNamesNoRoutePathIsReachedAtTheEndpointItself(string? cipRoutePath)
    {
        // Arrange
        var communication = DefaultTestCommunication() with { CipRoutePath = cipRoutePath };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.CipRoutePath.Should().BeNull();
    }

    [Fact]
    public void AConfigurationThatNamesNoPortReachesTheControllerOn44818()
    {
        // Arrange
        var communication = DefaultTestCommunication();

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.TcpPort.Should().Be(TcpPort.EtherNetIp);
    }

    [Fact]
    public void ADeviceKeepsTheConfigurationItWasMappedFrom()
    {
        // Arrange
        var communication = DefaultTestCommunication();

        // Act
        var rootNode = _mapper.CreateRootNode(communication);

        // Assert
        rootNode.OriginalCommunication.Should().BeSameAs(communication);
    }
}
