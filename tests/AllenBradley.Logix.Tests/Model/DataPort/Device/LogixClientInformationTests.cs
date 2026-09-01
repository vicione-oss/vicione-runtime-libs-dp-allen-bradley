using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.Device;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixClientTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.Device;

/// <summary>
/// The one string libplctag is handed to reach a controller. The address and the port are separate
/// properties everywhere else in the port, and this is where they stop being separate — a mistake here
/// is a handle pointed at the wrong socket.
/// </summary>
public sealed class LogixClientInformationTests
{
    [Fact]
    public void GatewayAttribute_JoinsTheEndpointAndThePortWithAColon()
    {
        // Arrange
        var clientInformation = CreateClientInformation("10.0.0.1");

        // Act
        var attribute = clientInformation.GatewayAttribute;

        // Assert
        attribute.Should().Be("10.0.0.1:44818");
    }

    /// <remarks>
    /// The registered port is written out rather than left off. libplctag would default to it either
    /// way, and a log line that says which socket a handle went to beats one that leaves it implied.
    /// </remarks>
    [Fact]
    public void GatewayAttribute_ForAMovedPort_CarriesThatPort()
    {
        // Arrange
        var clientInformation = CreateClientInformation("plc.example.local", 44819);

        // Act
        var attribute = clientInformation.GatewayAttribute;

        // Assert
        attribute.Should().Be("plc.example.local:44819");
    }
}
