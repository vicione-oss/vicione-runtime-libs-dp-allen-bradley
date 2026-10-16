using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;
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
    public void TheTagHandleOfALongElementIsNamedAfterTheDataFileAddress()
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation);
        var dataPoint = L9Element(0);

        // Act
        using var tagHandle = factory.CreateTagForAddress(dataPoint.Address);

        // Assert
        tagHandle.Name.Should().Be("L9:0");
    }

    [Fact]
    public void TheTagHandleOfAStringElementIsNamedAfterTheDataFileAddress()
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation);
        var dataPoint = ST9Element(1);

        // Act
        using var tagHandle = factory.CreateTagForAddress(dataPoint.Address);

        // Assert
        tagHandle.Name.Should().Be("ST9:1");
    }

    [Fact]
    public void TheTagHandleOfAnAsciiElementIsNamedAfterTheDataFileAddress()
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation);
        var dataPoint = A9Element(0);

        // Act
        using var tagHandle = factory.CreateTagForAddress(dataPoint.Address);

        // Assert
        tagHandle.Name.Should().Be("A9:0");
    }

    [Fact]
    public void TheTagHandleOfAnAsciiElementHoldsBothCharactersOfTheWord()
    {
        // Arrange
        // libplctag counts one byte per element of an ASCII file, so two elements make the one word.
        var factory = new LegacyTagAccessFactory(ClientInformation);

        // Act
        using var tagHandle = factory.CreateTagForAddress(A9Element(0).Address);

        // Assert
        tagHandle.ElementCount.Should().Be(2);
    }

    [Fact]
    public void TheTagHandleOfAnIntegerElementKeepsTheElementCountOfLibPlcTag()
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation);

        // Act
        using var tagHandle = factory.CreateTagForAddress(N7Element(0).Address);

        // Assert
        tagHandle.ElementCount.Should().BeNull();
    }

    [Fact]
    public void TheTagHandleOfAFloatElementIsNamedAfterTheDataFileAddress()
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation);
        var dataPoint = F8Element(12);

        // Act
        using var tagHandle = factory.CreateTagForAddress(dataPoint.Address);

        // Assert
        tagHandle.Name.Should().Be("F8:12");
    }

    [Fact]
    public void TheTagHandleOfABitIsNamedAfterTheElementAndTheBit()
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation);
        var dataPoint = B3Bit(2, 0);

        // Act
        using var tagHandle = factory.CreateTagForAddress(dataPoint.Address);

        // Assert
        tagHandle.Name.Should().Be("B3:2/0");
    }

    [Fact]
    public void ABitAddressGetsTheAccessThatReadsAndWritesOneBit()
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation);

        // Act
        using var access = factory.CreateLibPlcAccess(B3Bit(2, 0).Address);

        // Assert
        access.Should().BeOfType<LibPlcBitTagAccess>();
    }

    [Fact]
    public void AnElementAddressGetsTheAccessThatReadsAndWritesTheBuffer()
    {
        // Arrange
        var factory = new LegacyTagAccessFactory(ClientInformation);

        // Act
        using var access = factory.CreateLibPlcAccess(N7Element(0).Address);

        // Assert
        access.Should().BeOfType<LibPlcTagAccess>();
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
