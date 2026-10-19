using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Floats;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Integration.ab_server.AbServerDataFiles;
using static ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.TestData.LegacyDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Tests.Integration.ab_server;

/// <summary>
/// Writes one element of each data file on <see cref="AbServer"/> and reads it back. Each value has bytes that
/// differ from each other, so a swap of bytes or words cannot agree with the read-back.
/// </summary>
public sealed class DataFileRoundTripIntegrationTests(AbServer server) : AbServerIntegrationTestBase(server)
{
    [Fact]
    public async Task AnIntegerWrittenToAnIntegerFileIsReadBack()
    {
        // Arrange
        var element = new IntegerDataPoint(
            IntegerFile.Number, new ElementNumber(3), DefaultPollFrequency, NoChannels);

        // Act
        var values = await RoundTripAsync(element, (short)-12345);

        // Assert
        ILegacyDataPointValue[] expected = [element.CreateLegacyValue(-12345)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task ALongWrittenToALongFileIsReadBack()
    {
        // Arrange
        var element = new LongDataPoint(
            LongFile.Number, new ElementNumber(4), DefaultPollFrequency, NoChannels);

        // Act
        var values = await RoundTripAsync(element, -19_088_744);

        // Assert
        ILegacyDataPointValue[] expected = [element.CreateLegacyValue(-19_088_744)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task AFloatWrittenToAFloatFileIsReadBack()
    {
        // Arrange
        var element = new FloatDataPoint(
            FloatFile.Number, new ElementNumber(1), DefaultPollFrequency, NoChannels);

        // Act
        var values = await RoundTripAsync(element, -1234.5678f);

        // Assert
        ILegacyDataPointValue[] expected = [element.CreateLegacyValue(-1234.5678f)];
        values.Should().Equal(expected);
    }

    [Fact]
    public async Task AStringOfOddLengthWrittenToAStringFileIsReadBack()
    {
        // Arrange
        var element = new StringDataPoint(
            StringFile.Number, new ElementNumber(2), DefaultPollFrequency, NoChannels);

        // Act
        var values = await RoundTripAsync(element, "SLC 500");

        // Assert
        ILegacyDataPointValue[] expected = [element.CreateLegacyValue("SLC 500")];
        values.Should().Equal(expected);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ABitWrittenToABinaryFileIsReadBack(bool valueToWrite)
    {
        // Arrange
        var bit = new BitDataPoint(
            BinaryFile.Number, new ElementNumber(2), new BitNumber(5), DefaultPollFrequency, NoChannels);

        // Act
        var values = await RoundTripAsync(bit, valueToWrite);

        // Assert
        ILegacyDataPointValue[] expected = [bit.CreateLegacyValue(valueToWrite)];
        values.Should().Equal(expected);
    }
}
