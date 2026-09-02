using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixClientTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Access.LibPlcTag;

/// <summary>
/// The one string libplctag is handed to reach a controller. The address and the port are separate
/// properties everywhere else in the port, and this is where they stop being separate — a mistake here
/// is a handle pointed at the wrong socket.
/// </summary>
public sealed class GatewayAttributeTests
{
    [Fact]
    public void For_JoinsTheEndpointAndThePortWithAColon()
    {
        // Arrange
        var clientInformation = CreateClientInformation("10.0.0.1");

        // Act
        var attribute = GatewayAttribute.For(clientInformation);

        // Assert
        attribute.Value.Should().Be("10.0.0.1:44818");
    }

    /// <remarks>
    /// The registered port is written out rather than left off. libplctag would default to it either
    /// way, and a log line that says which socket a handle went to beats one that leaves it implied.
    /// </remarks>
    [Fact]
    public void For_AMovedPort_CarriesThatPort()
    {
        // Arrange
        var clientInformation = CreateClientInformation("plc.example.local", 44819);

        // Act
        var attribute = GatewayAttribute.For(clientInformation);

        // Assert
        attribute.Value.Should().Be("plc.example.local:44819");
    }
}
