using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.Nodes.Device.Mapping;
using ViciOne.Suite.DataPort.Extensions.Exceptions;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device.LogixControllerKind;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixCommunicationTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.Nodes.Device.Mapping;

public sealed class DeviceNodeMapperTests
{
    private readonly DeviceNodeMapper _mapper = new();

    /// <summary>Each device node type with the controller it stands for.</summary>
    public static TheoryData<string, LogixControllerKind> DeviceNodeTypes =>
        new()
        {
            { DeviceNode.ControlLogix5X70DesignId, ControlLogix5X70 },
            { DeviceNode.ControlLogix5X80DesignId, ControlLogix5X80 },
            { DeviceNode.CompactLogix5X70DesignId, CompactLogix5X70 },
            { DeviceNode.CompactLogix5X80DesignId, CompactLogix5X80 },
        };

    [Theory]
    [MemberData(nameof(DeviceNodeTypes))]
    public void ADeviceNodeTypeStandsForTheControllerItsDesignIdNames(
        string designId, LogixControllerKind expectedControllerKind)
    {
        // Arrange
        var communication = DefaultTestCommunication() with { DesignId = designId };

        // Act
        var deviceNode = _mapper.CreateRootNode(communication);

        // Assert
        deviceNode.ControllerKind.Should().Be(expectedControllerKind);
    }

    [Fact]
    public void ADeviceNodeTypeTheAddonDoesNotHaveIsRefusedByName()
    {
        // Arrange
        var communication = DefaultTestCommunication() with { DesignId = "DeviceMicro800" };

        // Act
        var mapping = _mapper.Invoking(mapper => mapper.CreateRootNode(communication));

        // Assert
        mapping.Should().Throw<InvalidConfigurationException>()
            .WithMessage("Unknown device design id: 'DeviceMicro800'.");
    }

    [Fact]
    public void ACompactLogixThatNamesNoRoutePathIsReachedAtSlotZeroOfTheVirtualBackplane()
    {
        // Arrange
        var communication = DefaultTestCommunication() with
        {
            DesignId = DeviceNode.CompactLogix5X70DesignId,
            CipRoutePath = null,
        };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        clientInformation.CipRoutePath.Value.Should().Be("1,0");
    }

    [Fact]
    public void EveryConnectionPropertyOfAConfigurationReachesTheClientInformation()
    {
        // Arrange
        var communication = DefaultTestCommunication() with
        {
            ConnectionEndpoint = "192.168.1.10",
            TcpPort = 44819,
            CipRoutePath = "1,2",
            OperationTimeout = 750,
        };

        // Act
        var clientInformation = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        var expected = new LogixClientInformation(
            new ConnectionEndpoint(communication.ConnectionEndpoint),
            new TcpPort(communication.TcpPort),
            new CipRoutePath(communication.CipRoutePath),
            new OperationTimeout(TimeSpan.FromMilliseconds(communication.OperationTimeout)));
        clientInformation.Should().Be(expected);
    }

    [Fact]
    public void TwoFamiliesAtOneAddressHaveEqualClientInformation()
    {
        // Arrange
        var controlLogix = DefaultTestCommunication() with { DesignId = DeviceNode.ControlLogix5X70DesignId };
        var compactLogix = DefaultTestCommunication() with { DesignId = DeviceNode.CompactLogix5X70DesignId };

        // Act
        var first = _mapper.CreateRootNode(controlLogix).ClientInformation;
        var second = _mapper.CreateRootNode(compactLogix).ClientInformation;

        // Assert
        first.Should().Be(second);
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

    [Fact]
    public void TheSameConfigurationTwiceHasEqualClientInformation()
    {
        // Arrange
        var communication = DefaultTestCommunication();

        // Act
        var first = _mapper.CreateRootNode(communication).ClientInformation;
        var second = _mapper.CreateRootNode(communication).ClientInformation;

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void AnotherConnectionEndpointHasDifferentClientInformation()
    {
        // Arrange
        var communication = DefaultTestCommunication();

        // Act
        var first = _mapper.CreateRootNode(communication).ClientInformation;
        var second = _mapper.CreateRootNode(communication with { ConnectionEndpoint = "10.0.0.2" }).ClientInformation;

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void AnotherTcpPortHasDifferentClientInformation()
    {
        // Arrange
        var communication = DefaultTestCommunication();

        // Act
        var first = _mapper.CreateRootNode(communication).ClientInformation;
        var second = _mapper.CreateRootNode(communication with { TcpPort = 44819 }).ClientInformation;

        // Assert
        first.Should().NotBe(second);
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
}
