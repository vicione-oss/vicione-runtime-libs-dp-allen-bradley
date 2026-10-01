using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Model.DataPort.DataPoints.Scalars.Counters;

public sealed class CounterDataPointTests
{
    private static readonly CounterDataPoint Parts = new(TagPath.Parse("Parts"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItNamesItselfAsStudio5000SpellsIt()
    {
        // Arrange

        // Act
        var dataTypeName = Parts.DataTypeName;

        // Assert
        dataTypeName.Should().Be(new DataTypeName("COUNTER"));
    }

    [Fact]
    public void ItIsAddressedAsTheCounterWithoutItsAccMember()
    {
        // Arrange

        // Act
        var tagAddress = Parts.TagAddress;

        // Assert
        tagAddress.Should().Be(new TagAddress("Parts"));
    }

    [Theory]
    [InlineData("Parts", "Parts.ACC")]
    [InlineData("Program:MainProgram.Parts", "Program:MainProgram.Parts.ACC")]
    [InlineData("Machine.Starts", "Machine.Starts.ACC")]
    public void ItsHandleReachesTheAccMemberOfTheCounter(string counterAddress, string expectedAddress)
    {
        // Arrange
        var counter = new CounterDataPoint(TagPath.Parse(counterAddress), DefaultPollFrequency, NoChannels);

        // Act
        var handleAddress = counter.HandleAddress;

        // Assert
        handleAddress.Should().Be(new TagAddress(expectedAddress));
    }

    [Fact]
    public void AnIntegerEngineValueIsCarriedThrough()
    {
        // Arrange

        // Act
        var conversion = Parts.ConvertValue(4711);

        // Assert
        conversion.Should().BeOfType<ConvertedDataPointValue<ILogixDataPointValue>>()
            .Which.Value.Value.Should().Be(4711);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void EveryAccumulatedCountIsInRange(int count)
    {
        // Arrange

        // Act
        var value = Parts.CreateTypedValue(count);

        // Assert
        value.IsInValueRange().Should().BeTrue();
    }
}
