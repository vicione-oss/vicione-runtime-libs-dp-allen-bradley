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
}
