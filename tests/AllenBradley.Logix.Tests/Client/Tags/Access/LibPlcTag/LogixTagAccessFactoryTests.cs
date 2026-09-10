using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
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
            DefaultTagName, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));

        // Act
        using var tag = _factory.CreateTagFor(readings);

        // Assert
        tag.ElementCount.Should().Be(DeclaredElementCount);
    }

    [Fact]
    public void TwoElementCountsAreTwoHandlesOfTheirOwnWidth()
    {
        // Arrange
        var readings = new IntArrayDataPoint(
            DefaultTagName, DefaultPollFrequency, NoChannels, new ElementCount(DeclaredElementCount));
        var moreReadings = readings with { ElementCount = new ElementCount(DeclaredElementCount * 2) };

        // Act
        using var tag = _factory.CreateTagFor(moreReadings);

        // Assert
        tag.ElementCount.Should().Be(DeclaredElementCount * 2);
    }

    /// <summary>One data point per shape that configures no extent: an elementary type, and a STRING.</summary>
    public static TheoryData<ILogixDataPoint> ScalarDataPoints =>
    [
        new IntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels),
        new StringDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels, StringMaxLength.Standard),
    ];

    [Theory]
    [MemberData(nameof(ScalarDataPoints))]
    public void AScalarTagLeavesTheElementCountToLibplctag(ILogixDataPoint dataPoint)
    {
        // Arrange

        // Act
        using var tag = _factory.CreateTagFor(dataPoint);

        // Assert
        tag.ElementCount.Should().BeNull("libplctag reads one element from a handle that does not say");
    }

    [Fact]
    public void ASchemaTagIsNeverSizedAsAnArray()
    {
        // Arrange
        // The @tags directory is a listing, not an array of the elements any data point is configured as.

        // Act
        using var tag = _factory.CreateTag(new TagName("@tags"));

        // Assert
        tag.ElementCount.Should().BeNull();
    }
}
