using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;
using static ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.TestData.LogixDataPointTestDataFactory;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Tests.Client.TypeConversion.Booleans;

/// <summary>
/// The <c>BOOL</c> codec — the one atomic whose decode is not a <c>BinaryPrimitives</c> read. The rule
/// is nonzero rather than equality with a pattern, so the byte the controller happens to store cannot
/// change the answer.
/// </summary>
public class BoolConverterTests
{
    private static readonly IDataPointConverter Converter = new BoolConverter();

    private static readonly BoolDataPoint Flag = new(new TagName("boolValue1"), DefaultPollFrequency, NoChannels);

    [Fact]
    public void ItExpectsTheControllerToDeclareTheTagABool()
    {
        // Act
        var expectedDataType = Converter.ExpectedDataType;

        // Assert
        // What LogixTypeComparison holds the symbol table against, and the one thing about this
        // converter a round trip cannot catch: reading a byte off a SINT succeeds and is wrong.
        expectedDataType.Should().Be(AllenBradleyDataType.Bool);
    }

    [Theory]
    [InlineData(0xFF)]
    [InlineData(0x01)]
    [InlineData(0x80)]
    public void Decode_ReadsAnyNonzeroByteAsTrue(byte bits)
    {
        // Arrange
        // 0xFF is what the controller stores for a set BOOL, but a member set through a mask can leave
        // any nonzero pattern behind.
        byte[] buffer = [bits];

        // Act
        var value = Converter.Decode(Flag, buffer);

        // Assert
        value.Value.Should().Be(true);
    }

    [Fact]
    public void Decode_ReadsAZeroByteAsFalse()
    {
        // Arrange
        byte[] buffer = [0x00];

        // Act
        var value = Converter.Decode(Flag, buffer);

        // Assert
        value.Value.Should().Be(false);
    }

    [Fact]
    public void Decode_ReadsOnlyTheOneByteTheTypeOccupies()
    {
        // Arrange
        byte[] buffer = [0x00, 0xFF, 0xFF, 0xFF];

        // Act
        var value = Converter.Decode(Flag, buffer);

        // Assert
        value.Value.Should().Be(false);
    }

    [Fact]
    public void Encode_WritesTrueAsTheByteStudio5000Shows()
    {
        // Arrange
        var value = Flag.CreateLogixValue(true);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        // Exactly the one byte a BOOL occupies: the batch copies these into the tag's buffer, so a
        // longer array would be a wider tag than the type declares.
        bytes.Should().Equal(0xFF);
    }

    [Fact]
    public void Encode_WritesFalseAsZero()
    {
        // Arrange
        var value = Flag.CreateLogixValue(false);

        // Act
        var bytes = Converter.Encode(value);

        // Assert
        bytes.Should().Equal(0x00);
    }
}
