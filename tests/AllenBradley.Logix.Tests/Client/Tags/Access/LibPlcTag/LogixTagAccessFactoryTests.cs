using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixClientTestDataFactory;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.Tags.Access.LibPlcTag;

/// <summary>
/// Every handle built here is disposed, initialised or not: under MTP a native handle left to its
/// finalizer fail-fasts the process on otherwise green tests.
/// </summary>
public sealed class LogixTagAccessFactoryTests
{
    private const int DeclaredElementCount = 10;

    private readonly LogixTagAccessFactory _factory = new(DefaultClientInformation());

    [Fact]
    public void AnArrayTagIsSizedToTheElementCountItWasConfiguredWith()
    {
        // Arrange
        var readings = new IntArrayDataPoint(
            DefaultTagPath, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

        // Act
        using var tag = _factory.CreateTagForDataPoint(readings);

        // Assert
        tag.ElementCount.Should().Be(DeclaredElementCount);
    }

    [Fact]
    public void TwoElementCountsAreTwoHandlesOfTheirOwnWidth()
    {
        // Arrange
        var readings = new IntArrayDataPoint(
            DefaultTagPath, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));
        var moreReadings = readings with { ElementCount = new ElementCount(DeclaredElementCount * 2) };

        // Act
        using var tag = _factory.CreateTagForDataPoint(moreReadings);

        // Assert
        tag.ElementCount.Should().Be(DeclaredElementCount * 2);
    }

    [Theory]
    [InlineData(32u, 1)]
    [InlineData(64u, 2)]
    public void ABoolArrayTagIsSizedToTheWordsItsBitsArePackedInto(uint declaredBitCount, int expectedWords)
    {
        // Arrange
        // libplctag puts the count on the request unchanged, and the controller counts a BOOL array in
        // the 32-bit words it allocated.
        var flags = new BoolArrayDataPoint(
            DefaultTagPath, DefaultPollFrequency, NoChannels, new ElementCount(declaredBitCount));

        // Act
        using var tag = _factory.CreateTagForDataPoint(flags);

        // Assert
        tag.ElementCount.Should().Be(expectedWords);
    }

    /// <summary>One data point per shape that configures no extent: an elementary type, a STRING and a TIMER.</summary>
    public static TheoryData<ILogixDataPoint> ScalarDataPoints =>
    [
        new IntDataPoint(DefaultTagPath, DefaultPollFrequency, NoChannels),
        new StringDataPoint(DefaultTagPath, DefaultPollFrequency, NoChannels, StringMaxLength.Standard),
        new TimerDataPoint(DefaultTagPath, DefaultPollFrequency, NoChannels),
    ];

    [Theory]
    [MemberData(nameof(ScalarDataPoints))]
    public void AScalarTagIsSizedToOneElementRatherThanLeftToLibplctag(ILogixDataPoint dataPoint)
    {
        // Arrange

        // Act
        using var tag = _factory.CreateTagForDataPoint(dataPoint);

        // Assert
        tag.ElementCount.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(ScalarDataPoints))]
    public void AHandleIsNamedAfterTheHandleAddressOfItsDataPoint(ILogixDataPoint dataPoint)
    {
        // Arrange

        // Act
        using var tag = _factory.CreateTagForDataPoint(dataPoint);

        // Assert
        tag.Name.Should().Be(dataPoint.HandleAddress.Value);
    }

    [Fact]
    public void ASchemaTagIsNeverSizedAsAnArray()
    {
        // Arrange
        // The @tags directory is a listing, not an array of the elements any data point is configured as.

        // Act
        using var tag = _factory.CreateTagForAddress(new TagAddress("@tags"));

        // Assert
        tag.ElementCount.Should().BeNull();
    }
}
