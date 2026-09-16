using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints;

public sealed class LogixDataPointTests
{
    [Fact]
    public void APointIsIdentifiedByTheTagItAddresses()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(
            TagPath.Parse("Program:MainProgram.Counter.PRE"), DefaultPollFrequency, NoChannels);

        // Act
        var identifier = dataPoint.Identifier;

        // Assert
        identifier.Should().Be(new DataPointIdentifier("Program:MainProgram.Counter.PRE"));
    }

    [Fact]
    public void APointAddressesTheTagItsPathRenders()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(TagPath.Parse("Program:Main.Readings[3]"), DefaultPollFrequency, NoChannels);

        // Act
        var tagAddress = dataPoint.TagAddress;

        // Assert
        tagAddress.Should().Be(new TagAddress("Program:Main.Readings[3]"));
    }

    [Fact]
    public void APointGivenAnotherPathAddressesThatPathsTag()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagPath, DefaultPollFrequency, NoChannels);

        // Act
        var moved = dataPoint with { TagPath = TagPath.Parse("Program:Main.Readings[3]") };

        // Assert
        moved.TagAddress.Should().Be(new TagAddress("Program:Main.Readings[3]"));
    }

    [Fact]
    public void AValueIsTheFrameworkViewOfItsPointAndPayload()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagPath, DefaultPollFrequency, NoChannels);

        // Act
        var value = dataPoint.CreateTypedValue(42);

        // Assert
        value.DataPoint.Should().BeSameAs(dataPoint);
        value.Value.Should().Be(42);
    }
}
