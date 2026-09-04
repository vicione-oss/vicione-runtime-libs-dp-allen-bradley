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
            new TagName("Program:MainProgram.Counter.PRE"), DefaultPollFrequency, NoChannels);

        // Act
        var identifier = dataPoint.Identifier;

        // Assert
        identifier.Should().Be(new DataPointIdentifier("Program:MainProgram.Counter.PRE"));
    }

    [Fact]
    public void AValueIsTheFrameworkViewOfItsPointAndPayload()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var value = dataPoint.CreateTypedValue(42);

        // Assert
        value.DataPoint.Should().BeSameAs(dataPoint);
        value.Value.Should().Be(42);
    }

    [Fact]
    public void AValueIsGoodByConstruction()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var value = dataPoint.CreateLogixValue(42);

        // Assert
        value.Quality.Should().Be(LogixQuality.Good);
    }

    [Fact]
    public void ABadValueCarriesThePointAndNoPayload()
    {
        // Arrange
        var dataPoint = new DIntDataPoint(DefaultTagName, DefaultPollFrequency, NoChannels);

        // Act
        var value = new BadLogixDataPointValue(dataPoint);

        // Assert
        value.Quality.Should().Be(LogixQuality.Bad);
        value.DataPoint.Should().BeSameAs(dataPoint);
        value.Value.Should().BeNull("a failed read has nothing to report, and default(int) is a value a tag can hold");
    }
}
