using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.Tags;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.Device;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyClientTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Client.Tags;

/// <summary>
/// Each test disposes the tag handle it creates. Under MTP, a handle left to its finalizer crashes the
/// test process, even when every test passes.
/// </summary>
public sealed class LegacyTagAccessFactoryTests
{
    private static readonly LegacyClientInformation ClientInformation = DefaultClientInformation();

    [Theory]
    [InlineData(LegacyControllerFamily.Slc500, PlcType.Slc500)]
    [InlineData(LegacyControllerFamily.MicroLogix, PlcType.MicroLogix)]
    public void TheControllerFamilySetsThePlcTypeOfTheTagHandle(
        LegacyControllerFamily family, PlcType expectedPlcType)
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation with { Family = family });

        // Act
        using var tagHandle = factory.CreateTagForAddress(N7Element(0).Address);

        // Assert
        tagHandle.PlcType.Should().Be(expectedPlcType);
    }

    [Fact]
    public void TheTagHandleIsNamedAfterTheDataFileAddress()
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation);
        var dataPoint = N7Element(12);

        // Act
        using var tagHandle = factory.CreateTagForAddress(dataPoint.Address);

        // Assert
        tagHandle.Name.Should().Be("N7:12");
    }

    [Fact]
    public void TheTagHandleConnectsToTheConfiguredEndpointAndTcpPort()
    {
        // Arrange
        var clientInformation = ClientInformation with
        {
            ConnectionEndpoint = new ConnectionEndpoint("10.0.0.1"),
            TcpPort = new TcpPort(2222),
        };
        var factory = new LegacyTagAccessFactory(clientInformation);

        // Act
        using var tagHandle = factory.CreateTagForAddress(N7Element(0).Address);

        // Assert
        tagHandle.Gateway.Should().Be("10.0.0.1:2222");
    }

    /// <summary>A route path with the path attribute of the tag handle.</summary>
    public static TheoryData<CipRoutePath, string?> RoutePaths =>
        new()
        {
            { new CipRoutePath("1,2,A:1:5"), "1,2,A:1:5" },
            { CipRoutePath.Direct, null },
        };

    [Theory]
    [MemberData(nameof(RoutePaths))]
    public void TheTagHandleGetsAPathOnlyForAConfiguredRoutePath(CipRoutePath cipRoutePath, string? expectedPath)
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation with { CipRoutePath = cipRoutePath });

        // Act
        using var tagHandle = factory.CreateTagForAddress(N7Element(0).Address);

        // Assert
        tagHandle.Path.Should().Be(expectedPath);
    }
}
